using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Models;

namespace FplPlanner.Service.Interface;

public interface IGameweekService
{
    Task<List<Gameweek>> GetAllAsync();
    Task<Gameweek> GetByIdAsync(Guid id);
    Task<Gameweek> GetByNumberAsync(int number);

    // First gameweek that is not finished; the one managers are planning for.
    Task<Gameweek> GetNextAsync();
    Task<List<Gameweek>> GetUpcomingAsync(int count);
    Task<Gameweek> InsertAsync(GameweekDto dto);
    Task<Gameweek> UpdateAsync(Guid id, GameweekDto dto);
    Task<Gameweek> DeleteAsync(Guid id);
}
