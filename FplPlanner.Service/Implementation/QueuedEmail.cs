using FplPlanner.Domain.Dto.Email;

namespace FplPlanner.Service.Implementation;

// An e-mail waiting in the queue, with an optional follow-up once it has been sent.
public record QueuedEmail(EmailMessage Message, Func<IServiceProvider, CancellationToken, Task>? OnSent);
