using Microsoft.Extensions.Options;
using FplPlanner.Domain.Configuration;
using RabbitMQ.Client;

namespace FplPlanner.Service.Implementation;

// One long-lived connection per application (RabbitMQ connections are expensive; channels are cheap).
// After the first successful connect the client recovers the connection, its channels and consumers
// by itself when the broker restarts, so the connection is never replaced here.
public sealed class RabbitMqConnectionProvider : IAsyncDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;

    public RabbitMqConnectionProvider(IOptions<RabbitMqSettings> settings)
    {
        var s = settings.Value;
        _factory = new ConnectionFactory
        {
            HostName = s.HostName,
            Port = s.Port,
            UserName = s.UserName,
            Password = s.Password,
            AutomaticRecoveryEnabled = true
        };
    }

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection != null)
        {
            return _connection;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Throws while the broker is unreachable; the caller decides whether to retry.
            _connection ??= await _factory.CreateConnectionAsync(cancellationToken);
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }

        _lock.Dispose();
    }
}
