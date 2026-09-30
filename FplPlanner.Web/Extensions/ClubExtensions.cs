using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Models;
using FplPlanner.Web.Request;
using FplPlanner.Web.Response;

namespace FplPlanner.Web.Extensions;

public static class ClubExtensions
{
    public static ClubResponse ToResponse(this Club club)
    {
        return new ClubResponse(club.Id, club.FplId, club.Name, club.ShortName, club.StrengthHome, club.StrengthAway);
    }

    public static List<ClubResponse> ToResponse(this IEnumerable<Club> clubs)
    {
        return clubs.Select(c => c.ToResponse()).ToList();
    }

    public static ClubDto ToDto(this ClubRequest request)
    {
        return new ClubDto
        {
            FplId = request.FplId,
            Name = request.Name,
            ShortName = request.ShortName.ToUpperInvariant(),
            StrengthHome = request.StrengthHome,
            StrengthAway = request.StrengthAway
        };
    }
}
