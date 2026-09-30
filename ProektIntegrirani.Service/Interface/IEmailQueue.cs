using ProektIntegrirani.Domain.Dto.Email;

namespace ProektIntegrirani.Service.Interface;

public interface IEmailQueue
{
    Task EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
