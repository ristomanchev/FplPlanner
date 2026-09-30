using ProektIntegrirani.Domain.Dto;

namespace ProektIntegrirani.Web.Extensions;

public static class ImportExtensions
{
    public static List<SquadPickInputDto> ToDto(this IEnumerable<SquadPickImportDto> records)
    {
        return records.Select(r => new SquadPickInputDto
        {
            PlayerId = r.PlayerId,
            SquadPosition = r.SquadPosition,
            IsCaptain = r.IsCaptain,
            IsViceCaptain = r.IsViceCaptain
        }).ToList();
    }
}
