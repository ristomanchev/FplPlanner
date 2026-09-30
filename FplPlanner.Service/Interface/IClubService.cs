using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Models;

namespace FplPlanner.Service.Interface;

public interface IClubService
{
    Task<List<Club>> GetAllAsync();
    Task<Club> GetByIdAsync(Guid id);
    Task<Club> InsertAsync(ClubDto dto);
    Task<Club> UpdateAsync(Guid id, ClubDto dto);
    Task<Club> DeleteAsync(Guid id);
}
