using FplPlanner.Domain.Dto;
using FplPlanner.Domain.Models;

namespace FplPlanner.Service.Interface;

public interface IFixtureService
{
    Task<List<Fixture>> GetAllAsync(int? gameweekNumber, Guid? clubId);
    Task<Fixture> GetByIdAsync(Guid id);
    Task<Fixture> InsertAsync(FixtureDto dto);
    Task<Fixture> UpdateAsync(Guid id, FixtureDto dto);
    Task<Fixture> DeleteAsync(Guid id);
}
