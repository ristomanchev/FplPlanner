using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Domain.Models;

namespace ProektIntegrirani.Service.Interface;

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
