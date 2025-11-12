using CommonObject;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqBroker;

public class DefaultRabbitMqPersistentConnection(
    IConnectionFactory connectionFactory,
    ILogger<DefaultRabbitMqPersistentConnection> logger,
    string[] hosts)
    : IRabbitMqPersistentConnection
{
    private readonly IConnectionFactory _connectionFactory =
        connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    private readonly ILogger<DefaultRabbitMqPersistentConnection> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private static IConnection? _connection;
    bool _disposed;
    private readonly Lock _syncRoot = new();
    public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

    public async Task<IChannel> CreateChannel()
    {
        if (!IsConnected)
        {
            await TryConnect();
        }

        if (!IsConnected)
        {
            throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
        }

        return await _connection!.CreateChannelAsync();
    }

    public string GetHosts()
    {
        return hosts.AsArrayJoin();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            _connection?.Dispose();
        }
        catch (IOException ex)
        {
            _logger.LogError("IOException {Message}", ex.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception {Message}", ex.ToString());
        }
    }

    public async Task<bool> TryConnect()
    {
        //lock (_syncRoot)
        {
            if (IsConnected)
            {
                _logger.LogInformation("1.RabbitMQ Client is connected");
                return true;
            }

            if (_connection == null)
            {
                _logger.LogInformation("RabbitMQ Client is null");
            }

            if (_connection is not { IsOpen: true })
            {
                _logger.LogInformation("RabbitMQ Client is closed");
            }

            if (_disposed)
            {
                _logger.LogInformation("RabbitMQ Client is disposed");
            }

            _logger.LogInformation("RabbitMQ Client is trying to connect");
            if (IsConnected)
            {
                _logger.LogInformation("2.RabbitMQ Client is connected");
            }
            else
            {
                try
                {
                    _connection = await _connectionFactory.CreateConnectionAsync(hosts);
                }  
                catch (Exception e)
                {
                    _logger.LogError(e, "RabbitMQ Client failed to connect");
                    throw;
                }
            }
            // var policy = Policy.Handle<SocketException>()
            //     .Or<BrokerUnreachableException>()
            //     .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            //         (ex, time) => { _logger.LogWarning("{Message}", ex.ToString()); }
            //     );
            // policy.Execute(async () =>
            // {
            //     if (IsConnected)
            //     {
            //         _logger.LogInformation("2.RabbitMQ Client is connected");
            //     }
            //     else
            //     {
            //         try
            //         {
            //             _connection = await _connectionFactory.CreateConnectionAsync(hosts);
            //         }
            //         catch (Exception e)
            //         {
            //             _logger.LogError(e, "RabbitMQ Client failed to connect");
            //             throw;
            //         }
            //     }
            // });

            if (IsConnected)
            {
                _connection!.ConnectionShutdownAsync += OnConnectionShutdown;
                _connection.CallbackExceptionAsync += OnCallbackException;
                _connection.ConnectionBlockedAsync += OnConnectionBlocked;
                _logger.LogInformation("{Message}",
                    $"RabbitMQ persistent connection acquired a connection {_connection?.Endpoint.HostName} and is subscribed to failure events");
                return true;
            }

            _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
            return false;
        }
    }

    private async Task OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
    {
        _logger.LogWarning("A RabbitMQ connection is blocked. Trying to re-connect...");
        _logger.LogWarning("A RabbitMQ, block reason: {@E}", e);
        if (_disposed)
        {
            _logger.LogInformation("OnConnectionBlocked: RabbitMQ Client is disposed");
            return;
        }

        if (IsConnected)
        {
            _logger.LogInformation("OnConnectionBlocked: RabbitMQ Client is connected");
            return;
        }

        await TryConnect();
    }

    private async Task OnCallbackException(object sender, CallbackExceptionEventArgs e)
    {
        _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");
        _logger.LogWarning("A RabbitMQ, callback exception reason: {@E}", e);
        if (_disposed)
        {
            _logger.LogInformation("OnConnectionBlocked: RabbitMQ Client is disposed");
            return;
        }

        if (IsConnected)
        {
            _logger.LogInformation("OnCallbackException: RabbitMQ Client is connected");
            return;
        }

        await TryConnect();
    }

    private async Task OnConnectionShutdown(object sender, ShutdownEventArgs reason)
    {
        _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
        _logger.LogWarning("A RabbitMQ, shutdown reason: {Reason}", reason);
        if (_disposed)
        {
            _logger.LogInformation("OnConnectionBlocked: RabbitMQ Client is disposed");
            return;
        }

        if (IsConnected)
        {
            _logger.LogInformation("OnConnectionShutdown: RabbitMQ Client is connected");
            return;
        }

        await TryConnect();
    }
}