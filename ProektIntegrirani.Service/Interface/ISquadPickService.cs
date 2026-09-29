using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Models;

namespace ProektIntegrirani.Service.Interface;

public interface ISquadPickService
{
    Task<List<SquadPick>> GetAllAsync(Guid? managerId, int? gameweekNumber);
    Task<SquadPick> GetByIdAsync(Guid id);
    Task<SquadPick> InsertAsync(SquadPickDto dto);
    Task<SquadPick> UpdateAsync(Guid id, SquadPickDto dto);
    Task<SquadPick> DeleteAsync(Guid id);
}
