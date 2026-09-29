namespace ProektIntegrirani.Service.Interface;

public interface IMessagePublisher
{
    // Serializes the message as JSON and puts it on a durable queue.
    Task PublishAsync<T>(string queue, T message, CancellationToken cancellationToken = default);
}
