using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProektIntegrirani.Domain.Configuration;
using ProektIntegrirani.Domain.Messages;
using ProektIntegrirani.Service.Implementation;
using ProektIntegrirani.Service.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProektIntegrirani.Service.Jobs;

// Listens for FplDataSyncedMessage and recalculates predictions asynchronously,
// so the ETL request returns as soon as the data is stored.
public class PredictionRecalculationConsumer : BackgroundService
{
    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<PredictionRecalculationConsumer> _logger;

    public PredictionRecalculationConsumer(RabbitMqConnectionProvider connectionProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> settings,
        ILogger<PredictionRecalculationConsumer> logger)
    {
        _connectionProvider = connectionProvider;
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // The broker may start after the application (or restart); keep trying until it is reachable.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning("RabbitMQ is not reachable ({Message}); retrying in {Delay}s.",
                    ex.Message, _settings.ReconnectDelaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(_settings.ReconnectDelaySeconds), stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        var connection = await _connectionProvider.GetConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        var queue = _settings.FplDataSyncedQueue;
        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: stoppingToken);
        // One message at a time: a recalculation is heavy and they would overwrite each other anyway.
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<FplDataSyncedMessage>(delivery.Body.Span)
                              ?? throw new JsonException("Empty message.");
                _logger.LogInformation("Received {MessageId} (synced at {SyncedAt}); recalculating predictions.",
                    message.MessageId, message.SyncedAt);

                using var scope = _scopeFactory.CreateScope();
                var predictionService = scope.ServiceProvider.GetRequiredService<IPredictionService>();
                await predictionService.RecalculateAsync(cancellationToken: stoppingToken);

                await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Not requeued: a message that failed once (bad payload, no gameweeks) would fail forever.
                _logger.LogError(ex, "Failed to process message {DeliveryTag}.", delivery.DeliveryTag);
                await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(queue, autoAck: false, consumer, stoppingToken);
        _logger.LogInformation("Listening on queue {Queue}.", queue);

        // Keep the channel open until the application stops.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
