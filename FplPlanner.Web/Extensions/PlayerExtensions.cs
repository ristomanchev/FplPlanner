using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Models;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Extensions;

public static class PlayerExtensions
{
    public static PlayerResponse ToResponse(this Player player)
    {
        var stats = player.Stats;
        return new PlayerResponse(
            player.Id,
            player.FplId,
            player.FirstName,
            player.LastName,
            player.WebName,
            player.Position.ToString(),
            player.Price,
            player.Status.ToString(),
            player.ChanceOfPlaying,
            player.News,
            player.ClubId,
            player.Club.ShortName,
            new PlayerStatsResponse(stats.TotalPoints, stats.Minutes, stats.Starts, stats.GoalsScored,
                stats.Assists, stats.ExpectedGoals, stats.ExpectedAssists, stats.Saves, stats.Bonus,
                stats.DefensiveContribution));
    }

    public static PlayerDto ToDto(this PlayerRequest request)
    {
        return new PlayerDto
        {
            FplId = request.FplId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            WebName = request.WebName,
            Position = request.Position,
            Price = request.Price,
            Status = request.Status,
            ChanceOfPlaying = request.ChanceOfPlaying,
            News = request.News,
            ClubId = request.ClubId
        };
    }

    public static PlayerFilterDto ToDto(this PlayerFilterRequest request)
    {
        return new PlayerFilterDto
        {
            Position = request.Position,
            ClubId = request.ClubId,
            MaxPrice = request.MaxPrice,
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search
        };
    }
}
