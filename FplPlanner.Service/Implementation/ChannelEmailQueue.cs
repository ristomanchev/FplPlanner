using System.Threading.Channels;
using FplPlanner.Domain.Dto.Email;
using FplPlanner.Service.Interface;

namespace FplPlanner.Service.Implementation;

// In-memory queue: the request only writes the message, EmailBackgroundService sends it.
public class ChannelEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel;

    public ChannelEmailQueue(Channel<EmailMessage> channel)
    {
        _channel = channel;
    }

    public async Task EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(message, cancellationToken);
    }
}
