using ProektIntegrirani.Domain.Dto;

namespace ProektIntegrirani.Service.Interface;

// Squad-level business logic: the whole 15-man squad of a manager in a gameweek.
public interface ISquadService
{
    // gameweekNumber = null means the manager's latest stored squad.
    Task<SquadDto> GetSquadAsync(Guid managerId, int? gameweekNumber);
    Task<SquadDto> SaveSquadAsync(Guid managerId, int gameweekNumber, List<SquadPickInputDto> picks);
    Task<SquadValidationResultDto> ValidateAsync(Guid managerId, int? gameweekNumber);
    Task<LineupDto> GetBestLineupAsync(Guid managerId);
    Task<TransferAdviceDto> GetTransferAdviceAsync(Guid managerId, int horizon, int maxTransfers);
}
