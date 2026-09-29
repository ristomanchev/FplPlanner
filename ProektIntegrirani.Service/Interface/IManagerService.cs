using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Models;

namespace ProektIntegrirani.Service.Interface;

public interface IManagerService
{
    Task<List<Manager>> GetAllAsync();
    Task<Manager> GetByIdAsync(Guid id);
    Task<Manager> InsertAsync(ManagerDto dto);
    Task<Manager> UpdateAsync(Guid id, ManagerDto dto);
    Task<Manager> DeleteAsync(Guid id);
}
