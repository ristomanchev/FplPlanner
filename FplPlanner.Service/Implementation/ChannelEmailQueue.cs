using System.Threading.Channels;
using FplPlanner.Domain.Dto.Email;
using FplPlanner.Domain.Exceptions;
using FplPlanner.Service.Interface;
using FplPlanner.Service.Logic;

namespace FplPlanner.Service.Implementation;

// In-memory queue: the request only writes the message, EmailBackgroundService sends it.
public class ChannelEmailQueue : IEmailQueue
{
    private readonly Channel<QueuedEmail> _channel;

    public ChannelEmailQueue(Channel<QueuedEmail> channel)
    {
        _channel = channel;
    }

    public async Task EnqueueAsync(EmailMessage message,
        Func<IServiceProvider, CancellationToken, Task>? onSent = null,
        CancellationToken cancellationToken = default)
    {
        // Rejected here, while the caller can still report it, instead of failing later in the background.
        if (!EmailAddressValidator.IsValid(message.To))
        {
            throw new BusinessRuleException($"'{message.To}' is not a valid e-mail address.");
        }

        if (string.IsNullOrWhiteSpace(message.Subject) || string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            throw new BusinessRuleException("An e-mail needs a subject and a body.");
        }

        await _channel.Writer.WriteAsync(new QueuedEmail(message, onSent), cancellationToken);
    }
}
