using FplPlanner.Domain.Dto.Email;

namespace FplPlanner.Service.Interface;

public interface IEmailQueue
{
    Task EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
