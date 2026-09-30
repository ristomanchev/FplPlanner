using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Extensions;

public static class SquadExtensions
{
    public static SquadResponse ToResponse(this SquadDto squad)
    {
        return new SquadResponse(
            squad.ManagerId,
            squad.TeamName,
            squad.GameweekNumber,
            squad.Bank,
            squad.SquadValue,
            squad.Members.Select(m => m.ToResponse()).ToList(),
            squad.Validation.ToResponse());
    }

    public static SquadMemberResponse ToResponse(this SquadMemberDto member)
    {
        return new SquadMemberResponse(
            member.PlayerId,
            member.FplId,
            member.WebName,
            member.Position.ToString(),
            member.ClubShortName,
            member.Price,
            member.Status.ToString(),
            member.ChanceOfPlaying,
            member.News,
            member.SquadPosition,
            member.IsCaptain,
            member.IsViceCaptain,
            member.ExpectedPoints);
    }

    public static SquadValidationResponse ToResponse(this SquadValidationResultDto validation)
    {
        return new SquadValidationResponse(validation.IsValid, validation.Errors);
    }

    public static LineupResponse ToResponse(this LineupDto lineup)
    {
        // e.g. "3-4-3": defenders-midfielders-forwards in the starting XI.
        var formation = string.Join("-", new[] { Position.Defender, Position.Midfielder, Position.Forward }
            .Select(p => lineup.StartingEleven.Count(m => m.Position == p)));

        return new LineupResponse(
            lineup.GameweekNumber,
            formation,
            lineup.StartingEleven.Select(m => m.ToResponse()).ToList(),
            lineup.Bench.Select(m => m.ToResponse()).ToList(),
            lineup.Captain?.ToResponse(),
            lineup.ViceCaptain?.ToResponse(),
            lineup.ExpectedPoints);
    }

    public static TransferAdviceResponse ToResponse(this TransferAdviceDto advice)
    {
        return new TransferAdviceResponse(
            advice.ManagerId,
            advice.GameweekNumbers,
            advice.FreeTransfers,
            advice.Bank,
            advice.CurrentScore,
            advice.RecommendedPlan.ToResponse(),
            advice.Plans.Select(p => p.ToResponse()).ToList(),
            advice.BestSingleTransfers.Select(t => t.ToResponse()).ToList());
    }

    public static TransferPlanResponse ToResponse(this TransferPlanDto plan)
    {
        return new TransferPlanResponse(
            plan.Transfers.Count,
            plan.Transfers.Select(t => t.ToResponse()).ToList(),
            plan.Gain,
            plan.HitCost,
            plan.NetGain,
            plan.BankAfter);
    }

    public static TransferResponse ToResponse(this TransferDto transfer)
    {
        return new TransferResponse(
            transfer.PlayerOut.ToResponse(),
            transfer.PlayerIn.ToResponse(),
            transfer.Gain,
            transfer.BankAfter);
    }

    public static PredictionRunResponse ToResponse(this PredictionRunResultDto result)
    {
        return new PredictionRunResponse(result.GameweekNumbers, result.PlayersEvaluated, result.PredictionsSaved,
            result.CalculatedAt);
    }

    public static List<SquadPickInputDto> ToDto(this SaveSquadRequest request)
    {
        return request.Picks.Select(p => new SquadPickInputDto
        {
            PlayerId = p.PlayerId,
            SquadPosition = p.SquadPosition,
            IsCaptain = p.IsCaptain,
            IsViceCaptain = p.IsViceCaptain
        }).ToList();
    }
}
