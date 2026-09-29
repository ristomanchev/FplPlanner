using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Extensions;

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
