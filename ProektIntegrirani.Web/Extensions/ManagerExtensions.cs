using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Web.Request;
using ProektIntegrirani.Web.Response;

namespace ProektIntegrirani.Web.Extensions;

public static class ManagerExtensions
{
    public static ManagerResponse ToResponse(this Manager manager)
    {
        return new ManagerResponse(manager.Id, manager.FplEntryId, manager.TeamName, manager.ManagerName,
            manager.Email, manager.Bank, manager.FreeTransfers, manager.LastReportedGameweek,
            manager.CreatedBy, manager.DateCreated, manager.LastModifiedBy, manager.DateLastModified);
    }

    public static List<ManagerResponse> ToResponse(this IEnumerable<Manager> managers)
    {
        return managers.Select(m => m.ToResponse()).ToList();
    }

    public static ManagerDto ToDto(this ManagerRequest request)
    {
        return new ManagerDto
        {
            FplEntryId = request.FplEntryId,
            TeamName = request.TeamName,
            ManagerName = request.ManagerName,
            Email = request.Email,
            Bank = request.Bank,
            FreeTransfers = request.FreeTransfers
        };
    }
}
