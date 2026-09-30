using FplPlanner.Domain.Enums;
using FplPlanner.Domain.Models;
using FplPlanner.Domain.Rules;
using FplPlanner.Domain.ValueObjects;

namespace FplPlanner.Service.Logic;

public record PlayerGameweekPrediction(Guid PlayerId, int GameweekNumber, double ExpectedMinutes,
    PointsBreakdown Breakdown);

// Expected-points model:
//   1. Team attack/defence ratings from goals and xG in finished fixtures, shrunk toward
//      a prior from the FPL strength rating.
//   2. Expected goals for/against in every upcoming fixture (Poisson, with home advantage).
//   3. Player minutes from starts/minutes share and injury status.
//   4. Player per-90 rates (xG, xA, defensive contributions, saves, bonus, cards) shrunk
//      toward a position + price prior, scaled by the fixture.
//   5. Points from the FPL scoring rules.
public class PoissonPredictionModel
{
    private const double HomeAdvantage = 1.1;
    private const double AwayAdvantage = 0.9;
    private const double TeamPriorGames = 6;
    private const double DefaultLeagueGoals = 1.4;
    private const int RegularPlayerMinutes = 270;

    private readonly IReadOnlyList<int> _gameweekNumbers;
    private readonly double _leagueGoals;
    private readonly Dictionary<Guid, TeamRating> _teamRatings;
    private readonly Dictionary<Guid, Dictionary<int, List<FixtureOutlook>>> _fixturesByClub;
    private readonly Dictionary<Position, RatePriors> _priors;
    private readonly DateOnly? _seasonStart;

    public PoissonPredictionModel(IReadOnlyCollection<Club> clubs, IReadOnlyCollection<Player> players,
        IReadOnlyCollection<Fixture> fixtures, IReadOnlyDictionary<Guid, int> gameweekNumbersById,
        IReadOnlyList<int> gameweekNumbers, DateTime? seasonStart)
    {
        _gameweekNumbers = gameweekNumbers;
        _seasonStart = seasonStart is { } start ? DateOnly.FromDateTime(start) : null;

        var finished = fixtures.Where(f => f.IsFinished && f.HomeScore != null && f.AwayScore != null).ToList();
        _leagueGoals = CalculateLeagueGoals(finished);
        _teamRatings = CalculateTeamRatings(clubs, players, finished);
        _fixturesByClub = BuildFixtureOutlooks(clubs, fixtures, gameweekNumbersById);
        _priors = Enum.GetValues<Position>().ToDictionary(p => p, p => RatePriors.Fit(players, p));
    }

    public IEnumerable<PlayerGameweekPrediction> Predict(Player player)
    {
        if (!_teamRatings.TryGetValue(player.ClubId, out var team))
        {
            yield break;
        }

        var profile = PlayerProfile.Build(player, GamesAvailableTo(player, team), _priors[player.Position]);
        var teamAvgFor = _leagueGoals * team.Attack;
        var teamAvgAgainst = _leagueGoals * team.Defence;
        var avgCleanSheet = Math.Exp(-teamAvgAgainst);

        for (var offset = 0; offset < _gameweekNumbers.Count; offset++)
        {
            var gameweek = _gameweekNumbers[offset];
            var availability = Availability(player, offset);
            var breakdown = new Accumulator();
            var minutes = 0.0;

            foreach (var fixture in _fixturesByClub[player.ClubId][gameweek])
            {
                minutes += profile.MinutesPerGame * availability;
                AddFixturePoints(breakdown, player.Position, profile, fixture, availability,
                    teamAvgFor, teamAvgAgainst, avgCleanSheet);
            }

            yield return new PlayerGameweekPrediction(player.Id, gameweek, minutes, breakdown.ToBreakdown());
        }
    }

    private static void AddFixturePoints(Accumulator b, Position position, PlayerProfile p, FixtureOutlook f,
        double availability, double teamAvgFor, double teamAvgAgainst, double avgCleanSheet)
    {
        var pAppear = p.AppearanceProbability * availability;
        var p60 = p.SixtyMinuteProbability * availability;
        var fraction = p.MinutesPerGame * availability / 90;
        var attackRatio = f.LambdaFor / teamAvgFor;
        var cleanSheet = Math.Exp(-f.LambdaAgainst);

        b.Appearance += p60 * ScoringRules.LongPlay + Math.Max(0, pAppear - p60) * ScoringRules.ShortPlay;
        b.Goals += p.Xg90 * fraction * attackRatio * ScoringRules.Goal(position);
        b.Assists += p.Xa90 * fraction * attackRatio * ScoringRules.Assist;
        b.CleanSheet += cleanSheet * p60 * ScoringRules.CleanSheet(position);

        if (ScoringRules.GoalsConceded(position) != 0)
        {
            b.GoalsConceded += PoissonMath.ExpectedFloorDiv(f.LambdaAgainst * fraction, 2)
                               * ScoringRules.GoalsConceded(position);
        }

        if (position == Position.Goalkeeper)
        {
            var saveMean = p.Saves90 * fraction * (f.LambdaAgainst / teamAvgAgainst);
            b.Saves += PoissonMath.ExpectedFloorDiv(saveMean, 3) * ScoringRules.PerThreeSaves;
        }

        if (ScoringRules.DefensiveContribution(position) != 0)
        {
            var perStart = p.DefCon90 * (Math.Max(p.MinutesPerStart, 60) / 90);
            b.DefensiveContribution += p60
                                       * PoissonMath.AtLeast(ScoringRules.DefensiveContributionThreshold(position), perStart)
                                       * ScoringRules.DefensiveContribution(position);
        }

        // Defenders' bonus follows clean-sheet chances, attackers' follows the attacking outlook.
        var fixtureBoost = position is Position.Goalkeeper or Position.Defender
            ? Math.Sqrt(cleanSheet / avgCleanSheet)
            : Math.Sqrt(attackRatio);
        b.Bonus += p.Bonus90 * fraction * fixtureBoost * ScoringRules.BonusPoint;
        b.Cards += p.Yellow90 * fraction * ScoringRules.YellowCard;
    }

    // Team games the player could have featured in; mid-season signings only count games since joining.
    private int GamesAvailableTo(Player player, TeamRating team)
    {
        var joined = player.Stats.ClubJoinDate;
        if (joined == null || _seasonStart == null || joined <= _seasonStart)
        {
            return team.GamesPlayed;
        }

        return team.FixtureDates.Count(date => date >= joined);
    }

    // Probability the player is available `offset` gameweeks from now. FPL only gives the chance
    // of playing for the next round, so injuries and suspensions are assumed to ease over time.
    public static double Availability(Player player, int offset)
    {
        if (player.Status == PlayerStatus.Unavailable) return 0;

        var now = player.ChanceOfPlaying is { } chance
            ? chance / 100.0
            : player.Status switch
            {
                PlayerStatus.Available => 1,
                PlayerStatus.Doubtful => 0.5,
                _ => 0
            };

        if (offset == 0 || now >= 1) return now;

        double[] recovery = player.Status switch
        {
            PlayerStatus.Doubtful => [1, 1, 1],
            PlayerStatus.Suspended => [0.5, 0.9, 1],
            PlayerStatus.NotInSquad => [0, 0, 0],
            _ => [0.35, 0.6, 0.8]
        };
        var recovered = recovery[Math.Min(offset - 1, recovery.Length - 1)];
        return Math.Max(now, Math.Min(1, recovered));
    }

    private static double CalculateLeagueGoals(List<Fixture> finished)
    {
        var teamGames = finished.Count * 2;
        var goals = finished.Sum(f => f.HomeScore!.Value + f.AwayScore!.Value);
        return teamGames > 0 ? (double)goals / teamGames : DefaultLeagueGoals;
    }

    private Dictionary<Guid, TeamRating> CalculateTeamRatings(IReadOnlyCollection<Club> clubs,
        IReadOnlyCollection<Player> players, List<Fixture> finished)
    {
        var totalGames = finished.Count * 2;
        var totalXg = players.Sum(p => (double)p.Stats.ExpectedGoals);
        var leagueXg = totalGames > 0 && totalXg > 0 ? totalXg / totalGames : _leagueGoals;

        static double StrengthOf(Club c) => (c.StrengthHome + c.StrengthAway) / 2.0;
        var meanStrength = clubs.Count > 0 ? clubs.Average(StrengthOf) : 3;

        var ratings = new Dictionary<Guid, TeamRating>();
        foreach (var club in clubs)
        {
            var home = finished.Where(f => f.HomeClubId == club.Id).ToList();
            var away = finished.Where(f => f.AwayClubId == club.Id).ToList();
            var games = home.Count + away.Count;
            var goalsFor = home.Sum(f => f.HomeScore!.Value) + away.Sum(f => f.AwayScore!.Value);
            var goalsAgainst = home.Sum(f => f.AwayScore!.Value) + away.Sum(f => f.HomeScore!.Value);

            var clubPlayers = players.Where(p => p.ClubId == club.Id).ToList();
            var xgFor = clubPlayers.Sum(p => (double)p.Stats.ExpectedGoals);
            // A goalkeeper's xGC is the team's xG against while he was on the pitch.
            var xgAgainst = clubPlayers.Where(p => p.Position == Position.Goalkeeper)
                .Sum(p => (double)p.Stats.ExpectedGoalsConceded);

            var strengthDiff = StrengthOf(club) - meanStrength;
            var attack = Math.Clamp(1 + 0.18 * strengthDiff, 0.6, 1.5);
            var defence = Math.Clamp(1 - 0.18 * strengthDiff, 0.6, 1.5);

            if (games > 0)
            {
                var observedAttack = 0.35 * (goalsFor / (double)games / _leagueGoals)
                                     + 0.65 * (xgFor / games / leagueXg);
                var observedDefence = 0.35 * (goalsAgainst / (double)games / _leagueGoals)
                                      + 0.65 * ((xgAgainst > 0 ? xgAgainst : goalsAgainst) / games / leagueXg);
                attack = (observedAttack * games + attack * TeamPriorGames) / (games + TeamPriorGames);
                defence = (observedDefence * games + defence * TeamPriorGames) / (games + TeamPriorGames);
            }

            var fixtureDates = home.Concat(away)
                .Where(f => f.KickoffTime != null)
                .Select(f => DateOnly.FromDateTime(f.KickoffTime!.Value))
                .ToList();
            ratings[club.Id] = new TeamRating(attack, defence, games, fixtureDates);
        }

        return ratings;
    }

    private Dictionary<Guid, Dictionary<int, List<FixtureOutlook>>> BuildFixtureOutlooks(
        IReadOnlyCollection<Club> clubs, IReadOnlyCollection<Fixture> fixtures,
        IReadOnlyDictionary<Guid, int> gameweekNumbersById)
    {
        var outlooks = clubs.ToDictionary(c => c.Id,
            _ => _gameweekNumbers.ToDictionary(g => g, _ => new List<FixtureOutlook>()));

        foreach (var fixture in fixtures.Where(f => !f.IsFinished && f.GameweekId != null))
        {
            var gameweek = gameweekNumbersById[fixture.GameweekId!.Value];
            if (!_gameweekNumbers.Contains(gameweek)
                || !_teamRatings.TryGetValue(fixture.HomeClubId, out var home)
                || !_teamRatings.TryGetValue(fixture.AwayClubId, out var away))
            {
                continue;
            }

            var lambdaHome = _leagueGoals * home.Attack * away.Defence * HomeAdvantage;
            var lambdaAway = _leagueGoals * away.Attack * home.Defence * AwayAdvantage;
            outlooks[fixture.HomeClubId][gameweek].Add(new FixtureOutlook(lambdaHome, lambdaAway));
            outlooks[fixture.AwayClubId][gameweek].Add(new FixtureOutlook(lambdaAway, lambdaHome));
        }

        return outlooks;
    }

    private record TeamRating(double Attack, double Defence, int GamesPlayed, List<DateOnly> FixtureDates);

    // Expected goals for and against the player's team in one fixture.
    private record FixtureOutlook(double LambdaFor, double LambdaAgainst);

    // Least-squares line y = A + B * price, fitted per position; the prior for a per-90 rate.
    private record PriceLine(double A, double B)
    {
        public double At(double price) => Math.Max(0, A + B * price);

        public static PriceLine Fit(IReadOnlyList<(double X, double Y)> points)
        {
            if (points.Count == 0) return new PriceLine(0, 0);

            var meanX = points.Average(p => p.X);
            var meanY = points.Average(p => p.Y);
            var sxy = points.Sum(p => (p.X - meanX) * (p.Y - meanY));
            var sxx = points.Sum(p => (p.X - meanX) * (p.X - meanX));
            var slope = sxx > 0 ? sxy / sxx : 0;
            return new PriceLine(meanY - slope * meanX, slope);
        }
    }

    private record RatePriors(PriceLine Xg, PriceLine Xa, PriceLine DefCon, PriceLine Saves, PriceLine Bonus,
        PriceLine Yellow)
    {
        public static RatePriors Fit(IReadOnlyCollection<Player> players, Position position)
        {
            var regulars = players
                .Where(p => p.Position == position && p.Stats.Minutes >= RegularPlayerMinutes)
                .ToList();

            PriceLine FitRate(Func<Player, double> total) => PriceLine.Fit(regulars
                .Select(p => ((double)p.Price, total(p) / p.Stats.Minutes * 90))
                .ToList());

            return new RatePriors(
                FitRate(p => (double)p.Stats.ExpectedGoals),
                FitRate(p => (double)p.Stats.ExpectedAssists),
                FitRate(p => p.Stats.DefensiveContribution),
                FitRate(p => p.Stats.Saves),
                FitRate(p => p.Stats.Bonus),
                FitRate(p => p.Stats.YellowCards));
        }
    }

    private record PlayerProfile(double MinutesPerGame, double MinutesPerStart, double AppearanceProbability,
        double SixtyMinuteProbability, double Xg90, double Xa90, double DefCon90, double Saves90, double Bonus90,
        double Yellow90)
    {
        // How many minutes of evidence the prior is worth, per rate.
        private const double AttackPriorMinutes = 450;
        private const double DefencePriorMinutes = 270;
        private const double CardPriorMinutes = 900;

        public static PlayerProfile Build(Player player, int teamGames, RatePriors priors)
        {
            var stats = player.Stats;
            double minutes = stats.Minutes;
            double starts = stats.Starts;
            var price = (double)player.Price;

            var startRate = teamGames > 0 ? Math.Clamp(starts / teamGames, 0, 1) : 0;
            var minutesPerGame = teamGames > 0 ? Math.Clamp(minutes / teamGames, 0, 90) : 0;
            var minutesPerStart = starts > 0 ? Math.Clamp(minutes / starts, 0, 90) : 0;
            var subRate = teamGames > starts
                ? Math.Clamp((minutes - starts * Math.Min(minutesPerStart, 85)) / (25 * (teamGames - starts)), 0, 1)
                : 0;
            var appearance = startRate + (1 - startRate) * subRate;
            var sixty = startRate * (minutesPerStart >= 75 ? 0.95 : minutesPerStart >= 65 ? 0.85 : 0.6);

            // Observed per-90 rate, shrunk toward the price prior; players with few minutes lean on the prior.
            double Rate(double observedTotal, PriceLine prior, double priorMinutes)
            {
                var observed90 = minutes > 0 ? observedTotal / minutes * 90 : 0;
                return (observed90 * minutes + prior.At(price) * priorMinutes) / (minutes + priorMinutes);
            }

            return new PlayerProfile(
                minutesPerGame,
                minutesPerStart,
                appearance,
                sixty,
                Rate(0.85 * (double)stats.ExpectedGoals + 0.15 * stats.GoalsScored, priors.Xg, AttackPriorMinutes),
                Rate(0.85 * (double)stats.ExpectedAssists + 0.15 * stats.Assists, priors.Xa, AttackPriorMinutes),
                Rate(stats.DefensiveContribution, priors.DefCon, DefencePriorMinutes),
                player.Position == Position.Goalkeeper ? Rate(stats.Saves, priors.Saves, DefencePriorMinutes) : 0,
                Rate(stats.Bonus, priors.Bonus, AttackPriorMinutes),
                Rate(stats.YellowCards, priors.Yellow, CardPriorMinutes));
        }
    }

    private class Accumulator
    {
        public double Appearance, Goals, Assists, CleanSheet, GoalsConceded, Saves, DefensiveContribution, Bonus, Cards;

        public PointsBreakdown ToBreakdown() => new()
        {
            Appearance = Round(Appearance),
            Goals = Round(Goals),
            Assists = Round(Assists),
            CleanSheet = Round(CleanSheet),
            GoalsConceded = Round(GoalsConceded),
            Saves = Round(Saves),
            DefensiveContribution = Round(DefensiveContribution),
            Bonus = Round(Bonus),
            Cards = Round(Cards)
        };

        private static decimal Round(double value) => Math.Round((decimal)value, 2);
    }
}
