using FplPlanner.Domain.Dto.Email;

namespace FplPlanner.Service.Interface;

public interface IEmailQueue
{
    // onSent runs (in its own DI scope) only after the e-mail has actually been delivered to the SMTP server.
    Task EnqueueAsync(EmailMessage message,
        Func<IServiceProvider, CancellationToken, Task>? onSent = null,
        CancellationToken cancellationToken = default);
}
