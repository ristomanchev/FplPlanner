using FplPlanner.Domain.Dto.Email;

namespace FplPlanner.Service.Interface;

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
