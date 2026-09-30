using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Models;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Extensions;

public static class GameweekExtensions
{
    public static GameweekResponse ToResponse(this Gameweek gameweek)
    {
        return new GameweekResponse(gameweek.Id, gameweek.Number, gameweek.Deadline, gameweek.IsFinished);
    }

    public static List<GameweekResponse> ToResponse(this IEnumerable<Gameweek> gameweeks)
    {
        return gameweeks.Select(g => g.ToResponse()).ToList();
    }

    public static GameweekDto ToDto(this GameweekRequest request)
    {
        return new GameweekDto
        {
            Number = request.Number,
            Deadline = request.Deadline.ToUniversalTime(),
            IsFinished = request.IsFinished
        };
    }
}
