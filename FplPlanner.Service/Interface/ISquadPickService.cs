using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Models;

namespace FplPlanner.Service.Interface;

public interface ISquadPickService
{
    Task<List<SquadPick>> GetAllAsync(Guid? managerId, int? gameweekNumber);
    Task<SquadPick> GetByIdAsync(Guid id);
    Task<SquadPick> InsertAsync(SquadPickDto dto);
    Task<SquadPick> UpdateAsync(Guid id, SquadPickDto dto);
    Task<SquadPick> DeleteAsync(Guid id);
}
