using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Models;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Extensions;

public static class SquadPickExtensions
{
    public static SquadPickResponse ToResponse(this SquadPick pick)
    {
        return new SquadPickResponse(
            pick.Id,
            pick.ManagerId,
            pick.Manager.TeamName,
            pick.GameweekId,
            pick.Gameweek.Number,
            pick.PlayerId,
            pick.Player.WebName,
            pick.Player.Position.ToString(),
            pick.Player.Club.ShortName,
            pick.Player.Price,
            pick.SquadPosition,
            pick.SquadPosition > 11,
            pick.IsCaptain,
            pick.IsViceCaptain);
    }

    public static List<SquadPickResponse> ToResponse(this IEnumerable<SquadPick> picks)
    {
        return picks.Select(p => p.ToResponse()).ToList();
    }

    public static SquadPickDto ToDto(this SquadPickRequest request)
    {
        return new SquadPickDto
        {
            ManagerId = request.ManagerId,
            GameweekId = request.GameweekId,
            PlayerId = request.PlayerId,
            SquadPosition = request.SquadPosition,
            IsCaptain = request.IsCaptain,
            IsViceCaptain = request.IsViceCaptain
        };
    }
}
