using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Domain.ExternalModels;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Domain.ValueObjects;

namespace ProektIntegrirani.Service.Implementation;

// The "Transform" step of the ETL: FPL API models -> domain entities with deterministic Ids.
public static class FplTransformations
{
    public static Club ToClub(FplTeam team)
    {
        return new Club
        {
            Id = GuidHelper.FromExternalId(nameof(Club), team.Id),
            FplId = team.Id,
            Name = team.Name,
            ShortName = team.ShortName,
            StrengthHome = ToStrength(team.StrengthOverallHome),
            StrengthAway = ToStrength(team.StrengthOverallAway)
        };
    }

    public static Gameweek ToGameweek(FplEvent fplEvent)
    {
        return new Gameweek
        {
            Id = GuidHelper.FromExternalId(nameof(Gameweek), fplEvent.Id),
            Number = fplEvent.Id,
            Deadline = fplEvent.DeadlineTime.ToUniversalTime(),
            IsFinished = fplEvent.Finished
        };
    }

    public static Player ToPlayer(FplElement element)
    {
        return new Player
        {
            Id = GuidHelper.FromExternalId(nameof(Player), element.Id),
            FplId = element.Id,
            FirstName = element.FirstName,
            LastName = element.SecondName,
            WebName = element.WebName,
            Position = ToPosition(element.ElementType),
            Price = ToMillions(element.NowCost),
            Status = ToPlayerStatus(element.Status),
            ChanceOfPlaying = element.ChanceOfPlayingNextRound,
            News = ToNews(element.News),
            ClubId = GuidHelper.FromExternalId(nameof(Club), element.Team),
            Stats = ToStats(element)
        };
    }

    public static Fixture ToFixture(FplFixture fixture)
    {
        return new Fixture
        {
            Id = GuidHelper.FromExternalId(nameof(Fixture), fixture.Id),
            FplId = fixture.Id,
            // A postponed fixture has no gameweek until FPL reschedules it.
            GameweekId = fixture.Event is { } number ? GuidHelper.FromExternalId(nameof(Gameweek), number) : null,
            HomeClubId = GuidHelper.FromExternalId(nameof(Club), fixture.TeamH),
            AwayClubId = GuidHelper.FromExternalId(nameof(Club), fixture.TeamA),
            KickoffTime = fixture.KickoffTime?.ToUniversalTime(),
            HomeScore = fixture.TeamHScore,
            AwayScore = fixture.TeamAScore,
            IsFinished = fixture.Finished
        };
    }

    public static Position ToPosition(int elementType)
    {
        return elementType switch
        {
            1 => Position.Goalkeeper,
            2 => Position.Defender,
            3 => Position.Midfielder,
            4 => Position.Forward,
            _ => throw new ArgumentOutOfRangeException(nameof(elementType), elementType, "Unknown FPL element type.")
        };
    }

    public static PlayerStatus ToPlayerStatus(string status)
    {
        return status switch
        {
            "a" => PlayerStatus.Available,
            "d" => PlayerStatus.Doubtful,
            "i" => PlayerStatus.Injured,
            "s" => PlayerStatus.Suspended,
            "u" => PlayerStatus.Unavailable,
            "n" => PlayerStatus.NotInSquad,
            _ => PlayerStatus.Unavailable
        };
    }

    // FPL sends money in tenths of a million: now_cost 105 = £10.5m.
    public static decimal ToMillions(int tenths)
    {
        return tenths / 10m;
    }

    public static string? ToNews(string? news)
    {
        return string.IsNullOrWhiteSpace(news) ? null : news.Trim();
    }

    // Older seasons had a 1–5 strength; keep a neutral 3 when the API leaves it out.
    public static int ToStrength(int? strength)
    {
        return strength is >= 1 and <= 5 ? strength.Value : 3;
    }

    public static PlayerSeasonStats ToStats(FplElement element)
    {
        return new PlayerSeasonStats
        {
            TotalPoints = element.TotalPoints,
            Minutes = element.Minutes,
            Starts = element.Starts,
            GoalsScored = element.GoalsScored,
            Assists = element.Assists,
            ExpectedGoals = element.ExpectedGoals,
            ExpectedAssists = element.ExpectedAssists,
            ExpectedGoalsConceded = element.ExpectedGoalsConceded,
            Saves = element.Saves,
            Bonus = element.Bonus,
            YellowCards = element.YellowCards,
            DefensiveContribution = element.DefensiveContribution,
            ClubJoinDate = element.TeamJoinDate
        };
    }
}
