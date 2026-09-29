using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Extensions;

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
