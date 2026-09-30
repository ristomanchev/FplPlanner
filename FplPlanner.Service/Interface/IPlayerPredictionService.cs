using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Enums;
using FplPlanner.Domain.Models;

namespace FplPlanner.Service.Interface;

public interface IPlayerPredictionService
{
    Task<PaginatedResult<PlayerPrediction>> GetAllPagedAsync(int? gameweekNumber, Position? position,
        int pageNumber, int pageSize);
    Task<List<PlayerPrediction>> GetForGameweeksAsync(List<int> gameweekNumbers);
    Task<PlayerPrediction> GetByIdAsync(Guid id);
    Task<PlayerPrediction> InsertAsync(PlayerPredictionDto dto);
    Task<PlayerPrediction> UpdateAsync(Guid id, PlayerPredictionDto dto);
    Task<PlayerPrediction> DeleteAsync(Guid id);
}
