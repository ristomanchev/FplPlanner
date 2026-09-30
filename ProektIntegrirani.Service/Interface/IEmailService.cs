using ProektIntegrirani.Domain.Dto.Email;

namespace ProektIntegrirani.Service.Interface;

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
