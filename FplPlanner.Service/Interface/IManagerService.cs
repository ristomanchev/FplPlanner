using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Models;

namespace FplPlanner.Service.Interface;

public interface IManagerService
{
    Task<List<Manager>> GetAllAsync();
    Task<Manager> GetByIdAsync(Guid id);
    Task<Manager> InsertAsync(ManagerDto dto);
    Task<Manager> UpdateAsync(Guid id, ManagerDto dto);
    Task<Manager> DeleteAsync(Guid id);
}
