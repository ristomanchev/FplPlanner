using System.Text.Json;
using FplPlanner.Service.Interface;
using RabbitMQ.Client;

namespace FplPlanner.Service.Implementation;

public class RabbitMqMessagePublisher : IMessagePublisher
{
    private readonly RabbitMqConnectionProvider _connectionProvider;

    public RabbitMqMessagePublisher(RabbitMqConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task PublishAsync<T>(string queue, T message, CancellationToken cancellationToken = default)
    {
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        // Durable queue + persistent message: survives a broker restart.
        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            Persistent = true
        };
        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue, mandatory: false,
            basicProperties: properties, body: body, cancellationToken: cancellationToken);
    }
}
