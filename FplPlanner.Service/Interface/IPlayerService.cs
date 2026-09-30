using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Models;

namespace FplPlanner.Service.Interface;

public interface IPlayerService
{
    Task<PaginatedResult<Player>> GetAllPagedAsync(PlayerFilterDto filter, int pageNumber, int pageSize);
    Task<Player> GetByIdAsync(Guid id);
    Task<List<Player>> GetAllByFplIdsInAsync(List<int> fplIds);
    Task<Player> InsertAsync(PlayerDto dto);
    Task<Player> UpdateAsync(Guid id, PlayerDto dto);
    Task<Player> DeleteAsync(Guid id);
}
