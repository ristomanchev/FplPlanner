using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Enums;
using FplPlanner.Domain.Rules;

namespace FplPlanner.Service.Logic;

// Lineup and transfer search on top of expected points.
// Squad score = best valid XI + captain (counted twice) + a small weight for the bench.
public static class SquadOptimizer
{
    public const double DefaultBenchWeight = 0.1;
    private const double Epsilon = 1e-6;

    // Demand a clearly better net gain before recommending an extra -4 hit (model noise).
    private const double HitSafetyMargin = 2;
    private const double TransferSafetyMargin = 0.05;

    public record Lineup(List<SquadMemberDto> StartingEleven, List<SquadMemberDto> Bench,
        SquadMemberDto? Captain, SquadMemberDto? ViceCaptain, double StartingElevenScore, double Score);

    public static Lineup BestLineup(IReadOnlyList<SquadMemberDto> squad, Func<SquadMemberDto, double> scoreOf,
        double benchWeight = DefaultBenchWeight)
    {
        var byPosition = squad
            .GroupBy(m => m.Position)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(scoreOf).ToList());

        // The minimum of every position starts, the best remaining outfielders fill the XI.
        var startingEleven = new List<SquadMemberDto>();
        var rest = new List<SquadMemberDto>();
        foreach (var (position, (min, _)) in FplRules.StartingElevenLimits)
        {
            var players = byPosition.GetValueOrDefault(position) ?? [];
            startingEleven.AddRange(players.Take(min));
            rest.AddRange(players.Skip(min));
        }

        var benchGoalkeepers = rest.Where(m => m.Position == Position.Goalkeeper).ToList();
        var outfield = rest.Where(m => m.Position != Position.Goalkeeper).OrderByDescending(scoreOf).ToList();
        var freeSlots = FplRules.StartingElevenSize - startingEleven.Count;
        startingEleven.AddRange(outfield.Take(freeSlots));
        var bench = benchGoalkeepers.Concat(outfield.Skip(freeSlots)).ToList();

        var ranked = startingEleven.OrderByDescending(scoreOf).ToList();
        var captain = ranked.ElementAtOrDefault(0);
        var viceCaptain = ranked.ElementAtOrDefault(1);

        var elevenScore = startingEleven.Sum(scoreOf);
        var captainScore = captain != null ? scoreOf(captain) : 0;
        var benchScore = bench.Sum(scoreOf);

        return new Lineup(
            startingEleven.OrderBy(m => m.Position).ThenByDescending(scoreOf).ToList(),
            bench,
            captain,
            viceCaptain,
            elevenScore + captainScore,
            elevenScore + captainScore + benchWeight * benchScore);
    }

    public static TransferAdviceDto SuggestTransfers(IReadOnlyList<SquadMemberDto> squad,
        IReadOnlyList<SquadMemberDto> allPlayers, IReadOnlyDictionary<Guid, double> scores, decimal bank,
        int freeTransfers, int maxTransfers, int topSingles = 10)
    {
        double ScoreOf(SquadMemberDto m) => scores.GetValueOrDefault(m.PlayerId);
        double Evaluate(IReadOnlyList<SquadMemberDto> s) => BestLineup(s, ScoreOf).Score;

        var baseScore = Evaluate(squad);

        var singles = FindImprovingSwaps(squad, allPlayers, bank, locked: new HashSet<Guid>(), Evaluate)
            .OrderByDescending(s => s.Score)
            .Take(topSingles)
            .Select(s => ToTransfer(s.Out, s.In, s.Score - baseScore, s.BankAfter, ScoreOf))
            .ToList();

        var noTransfers = new TransferPlanDto { BankAfter = bank };
        var plans = new List<TransferPlanDto> { noTransfers };

        // Greedy plans: apply the best single swap, lock the incoming player, repeat.
        var current = squad.ToList();
        var currentBank = bank;
        var locked = new HashSet<Guid>();
        var transfers = new List<TransferDto>();
        for (var k = 1; k <= maxTransfers; k++)
        {
            var best = FindImprovingSwaps(current, allPlayers, currentBank, locked, Evaluate)
                .MaxBy(s => s.Score);
            if (best == null) break;

            var index = current.FindIndex(m => m.PlayerId == best.Out.PlayerId);
            current[index] = best.In;
            currentBank = best.BankAfter;
            locked.Add(best.In.PlayerId);
            transfers.Add(ToTransfer(best.Out, best.In, best.Score - baseScore, best.BankAfter, ScoreOf));

            plans.Add(new TransferPlanDto
            {
                Transfers = transfers.ToList(),
                Gain = Round(best.Score - baseScore),
                HitCost = Math.Max(0, k - freeTransfers) * FplRules.TransferHitCost,
                BankAfter = currentBank
            });
        }

        var recommended = plans.Aggregate((a, b) =>
        {
            var margin = b.HitCost > a.HitCost ? HitSafetyMargin : TransferSafetyMargin;
            return (double)b.NetGain > (double)a.NetGain + margin ? b : a;
        });

        return new TransferAdviceDto
        {
            FreeTransfers = freeTransfers,
            Bank = bank,
            CurrentScore = Round(baseScore),
            BestSingleTransfers = singles,
            Plans = plans,
            RecommendedPlan = recommended
        };
    }

    private record Swap(SquadMemberDto Out, SquadMemberDto In, double Score, decimal BankAfter);

    // Every single swap (same position, affordable, club limit kept) that improves the squad score.
    private static IEnumerable<Swap> FindImprovingSwaps(IReadOnlyList<SquadMemberDto> squad,
        IReadOnlyList<SquadMemberDto> allPlayers, decimal bank, IReadOnlySet<Guid> locked,
        Func<IReadOnlyList<SquadMemberDto>, double> evaluate)
    {
        var baseScore = evaluate(squad);
        var inSquad = squad.Select(m => m.PlayerId).ToHashSet();
        var clubCounts = squad.GroupBy(m => m.ClubId).ToDictionary(g => g.Key, g => g.Count());

        for (var i = 0; i < squad.Count; i++)
        {
            var playerOut = squad[i];
            if (locked.Contains(playerOut.PlayerId)) continue;

            var funds = bank + playerOut.Price;
            foreach (var playerIn in allPlayers)
            {
                if (playerIn.Position != playerOut.Position
                    || inSquad.Contains(playerIn.PlayerId)
                    || playerIn.Price > funds
                    || (playerIn.ClubId != playerOut.ClubId
                        && clubCounts.GetValueOrDefault(playerIn.ClubId) >= FplRules.MaxPlayersPerClub))
                {
                    continue;
                }

                var trial = squad.ToList();
                trial[i] = playerIn;
                var score = evaluate(trial);
                if (score > baseScore + Epsilon)
                {
                    yield return new Swap(playerOut, playerIn, score, funds - playerIn.Price);
                }
            }
        }
    }

    private static TransferDto ToTransfer(SquadMemberDto playerOut, SquadMemberDto playerIn, double gain,
        decimal bankAfter, Func<SquadMemberDto, double> scoreOf)
    {
        return new TransferDto
        {
            PlayerOut = WithScore(playerOut, scoreOf),
            PlayerIn = WithScore(playerIn, scoreOf),
            Gain = Round(gain),
            BankAfter = bankAfter
        };
    }

    private static SquadMemberDto WithScore(SquadMemberDto member, Func<SquadMemberDto, double> scoreOf)
    {
        return new SquadMemberDto
        {
            PlayerId = member.PlayerId,
            FplId = member.FplId,
            WebName = member.WebName,
            Position = member.Position,
            ClubId = member.ClubId,
            ClubShortName = member.ClubShortName,
            Price = member.Price,
            Status = member.Status,
            ExpectedPoints = Round(scoreOf(member))
        };
    }

    private static decimal Round(double value) => Math.Round((decimal)value, 2);
}
