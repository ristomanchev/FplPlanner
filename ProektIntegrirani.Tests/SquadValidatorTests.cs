using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Service.Logic;

namespace ProektIntegrirani.Tests;

public class SquadValidatorTests
{
    [Fact]
    public void ValidSquad_HasNoErrors()
    {
        var result = SquadValidator.Validate(SquadBuilder.ValidSquad());

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void FourteenPlayers_IsRejected()
    {
        var squad = SquadBuilder.ValidSquad();
        squad.RemoveAt(squad.Count - 1);

        var result = SquadValidator.Validate(squad);

        Assert.Contains(result.Errors, e => e.Contains("15 players"));
    }

    [Fact]
    public void WrongPositionShape_IsRejected()
    {
        var squad = SquadBuilder.ValidSquad();
        squad.Single(m => m.SquadPosition == 15).Position = Position.Midfielder; // 6 MID, 2 FWD

        var result = SquadValidator.Validate(squad);

        Assert.Contains(result.Errors, e => e.Contains("5 midfielders"));
        Assert.Contains(result.Errors, e => e.Contains("3 forwards"));
    }

    [Fact]
    public void FourPlayersFromOneClub_IsRejected()
    {
        var squad = SquadBuilder.ValidSquad();
        squad[1].ClubId = squad[0].ClubId;
        squad[1].ClubShortName = squad[0].ClubShortName;

        var result = SquadValidator.Validate(squad);

        Assert.Contains(result.Errors, e => e.Contains("At most 3 players"));
    }

    [Fact]
    public void StartingElevenWithTwoGoalkeepers_IsRejected()
    {
        var squad = SquadBuilder.ValidSquad();
        // Swap the bench goalkeeper (12) with a starting midfielder (9).
        squad.Single(m => m.SquadPosition == 12).SquadPosition = 99;
        squad.Single(m => m.SquadPosition == 9).SquadPosition = 12;
        squad.Single(m => m.SquadPosition == 99).SquadPosition = 9;

        var result = SquadValidator.Validate(squad);

        Assert.Contains(result.Errors, e => e.Contains("exactly 1 goalkeepers"));
    }

    [Fact]
    public void TwoDefendersInStartingEleven_IsRejected()
    {
        var squad = SquadBuilder.ValidSquad();
        // Swap a starting defender (4) with the bench forward (15): 2 DEF - 5 MID - 3 FWD.
        squad.Single(m => m.SquadPosition == 4).SquadPosition = 99;
        squad.Single(m => m.SquadPosition == 15).SquadPosition = 4;
        squad.Single(m => m.SquadPosition == 99).SquadPosition = 15;

        var result = SquadValidator.Validate(squad);

        Assert.Contains(result.Errors, e => e.Contains("3–5 defenders"));
    }

    [Fact]
    public void MissingCaptainAndCaptainOnBench_AreRejected()
    {
        var squad = SquadBuilder.ValidSquad();
        squad.Single(m => m.IsCaptain).IsCaptain = false;

        var noCaptain = SquadValidator.Validate(squad);
        Assert.Contains(noCaptain.Errors, e => e.Contains("exactly one captain"));

        squad.Single(m => m.SquadPosition == 13).IsCaptain = true;
        var benchCaptain = SquadValidator.Validate(squad);
        Assert.Contains(benchCaptain.Errors, e => e.Contains("must be in the starting XI"));
    }

    [Fact]
    public void DuplicateSquadPosition_IsRejected()
    {
        var squad = SquadBuilder.ValidSquad();
        squad.Single(m => m.SquadPosition == 14).SquadPosition = 13;

        var result = SquadValidator.Validate(squad);

        Assert.Contains(result.Errors, e => e.Contains("only once"));
    }

    [Fact]
    public void AllViolations_AreReportedTogether()
    {
        var squad = SquadBuilder.ValidSquad();
        squad.RemoveAt(0);                     // 14 players, one goalkeeper short
        squad.ForEach(m => m.IsViceCaptain = false);

        var result = SquadValidator.Validate(squad);

        Assert.True(result.Errors.Count >= 3);
    }
}
