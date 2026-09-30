using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Service.Logic;

namespace ProektIntegrirani.Tests;

public class SquadOptimizerTests
{
    [Fact]
    public void BestLineup_PicksHighestScoringValidEleven_AndCaptain()
    {
        var squad = SquadBuilder.ValidSquad();
        var scores = squad.ToDictionary(m => m.PlayerId, _ => 2.0);
        var benchForward = squad.Single(m => m.SquadPosition == 15);
        var benchDefender = squad.Single(m => m.SquadPosition == 13);
        scores[benchForward.PlayerId] = 9;   // should start and captain
        scores[benchDefender.PlayerId] = 7;  // should start and be vice

        var lineup = SquadOptimizer.BestLineup(squad, m => scores[m.PlayerId]);

        Assert.Equal(11, lineup.StartingEleven.Count);
        Assert.Equal(4, lineup.Bench.Count);
        Assert.Single(lineup.StartingEleven, m => m.Position == Position.Goalkeeper);
        Assert.Contains(benchForward, lineup.StartingEleven);
        Assert.Contains(benchDefender, lineup.StartingEleven);
        Assert.Equal(benchForward, lineup.Captain);
        Assert.Equal(benchDefender, lineup.ViceCaptain);

        // Ten starters at 2 points, plus 9 and 7, plus the captain's 9 again.
        Assert.Equal(9 * 2.0 + 9 + 7 + 9, lineup.StartingElevenScore, 6);
    }

    [Fact]
    public void BestLineup_KeepsTheFormationMinimums()
    {
        var squad = SquadBuilder.ValidSquad();
        // Every defender scores zero, but three of them must still start.
        var lineup = SquadOptimizer.BestLineup(squad,
            m => m.Position == Position.Defender ? 0 : 5);

        Assert.Equal(3, lineup.StartingEleven.Count(m => m.Position == Position.Defender));
    }

    [Fact]
    public void SuggestTransfers_FindsTheAffordableUpgrade_AndRespectsBudget()
    {
        var squad = SquadBuilder.ValidSquad();
        var weakMidfielder = squad.Single(m => m.SquadPosition == 9);
        var newClub = Guid.NewGuid();

        var affordableStar = Candidate(Position.Midfielder, price: 5.5m, newClub);
        var expensiveStar = Candidate(Position.Midfielder, price: 12m, newClub);
        var allPlayers = squad.Append(affordableStar).Append(expensiveStar).ToList();

        var scores = allPlayers.ToDictionary(m => m.PlayerId, _ => 3.0);
        scores[weakMidfielder.PlayerId] = 0;
        scores[affordableStar.PlayerId] = 8;
        scores[expensiveStar.PlayerId] = 20;

        var advice = SquadOptimizer.SuggestTransfers(squad, allPlayers, scores, bank: 1m, freeTransfers: 1,
            maxTransfers: 1);

        var transfer = Assert.Single(advice.RecommendedPlan.Transfers);
        Assert.Equal(weakMidfielder.PlayerId, transfer.PlayerOut.PlayerId);
        Assert.Equal(affordableStar.PlayerId, transfer.PlayerIn.PlayerId);
        Assert.Equal(0.5m, transfer.BankAfter);
        Assert.Equal(0, advice.RecommendedPlan.HitCost);
    }

    [Fact]
    public void SuggestTransfers_DoesNotBreakTheThreePerClubRule()
    {
        var squad = SquadBuilder.ValidSquad();
        var weakMidfielder = squad.Single(m => m.SquadPosition == 9);
        // A club that already has three players in the squad, but not the outgoing player's club.
        var fullClub = squad.Where(m => m.ClubId != weakMidfielder.ClubId)
            .GroupBy(m => m.ClubId).First(g => g.Count() == 3).Key;

        var candidate = Candidate(Position.Midfielder, price: 5m, fullClub);
        var allPlayers = squad.Append(candidate).ToList();
        var scores = allPlayers.ToDictionary(m => m.PlayerId, _ => 3.0);
        scores[weakMidfielder.PlayerId] = 0;
        scores[candidate.PlayerId] = 10;

        var advice = SquadOptimizer.SuggestTransfers(squad, allPlayers, scores, bank: 5m, freeTransfers: 1,
            maxTransfers: 1);

        // The candidate may only replace a player from its own (full) club, never the weak midfielder.
        Assert.NotEmpty(advice.BestSingleTransfers);
        Assert.All(advice.BestSingleTransfers.Where(t => t.PlayerIn.PlayerId == candidate.PlayerId),
            t => Assert.Equal(fullClub, t.PlayerOut.ClubId));
        Assert.DoesNotContain(advice.BestSingleTransfers, t => t.PlayerOut.PlayerId == weakMidfielder.PlayerId);
    }

    [Fact]
    public void SuggestTransfers_TakesAHitOnlyWhenItClearlyPaysOff()
    {
        var squad = SquadBuilder.ValidSquad();
        var outA = squad.Single(m => m.SquadPosition == 8);
        var outB = squad.Single(m => m.SquadPosition == 9);
        var inA = Candidate(Position.Midfielder, 5m, Guid.NewGuid());
        var inB = Candidate(Position.Midfielder, 5m, Guid.NewGuid());
        var allPlayers = squad.Append(inA).Append(inB).ToList();

        var scores = allPlayers.ToDictionary(m => m.PlayerId, _ => 3.0);
        scores[outA.PlayerId] = 0;
        scores[outB.PlayerId] = 0;
        scores[inA.PlayerId] = 8;
        scores[inB.PlayerId] = 4.5; // the second transfer gains less than its 4-point hit

        var advice = SquadOptimizer.SuggestTransfers(squad, allPlayers, scores, bank: 0m, freeTransfers: 1,
            maxTransfers: 2);

        Assert.Equal(3, advice.Plans.Count); // 0, 1 and 2 transfers
        Assert.Equal(4, advice.Plans[2].HitCost);
        Assert.Single(advice.RecommendedPlan.Transfers);
    }

    private static SquadMemberDto Candidate(Position position, decimal price, Guid clubId) => new()
    {
        PlayerId = Guid.NewGuid(),
        WebName = "Candidate",
        Position = position,
        ClubId = clubId,
        ClubShortName = "NEW",
        Price = price,
        Status = PlayerStatus.Available
    };
}
