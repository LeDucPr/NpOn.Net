using CommonObject.CommonObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace RabbitMqBroker;

public class RabbitMqConnectionPool
{
    private readonly string[] _rabbitMqHosts;
    private readonly int _poolSize;
    private readonly string _virtualHost;
    private readonly string _rabbitMqUserName;
    private readonly string _rabbitMqPassword;
    private readonly ILogger<RabbitMqConnectionPool> _logger;

    public RabbitMqConnectionPool(ILogger<RabbitMqConnectionPool> logger,
        int poolSize,
        string[]? rabbitMqHosts,
        string virtualHost,
        string rabbitMqUserName, string rabbitMqPassword
    )
    {
        _logger = logger;
        _poolSize = poolSize;
        _virtualHost = virtualHost;
        _rabbitMqUserName = rabbitMqUserName;
        _rabbitMqPassword = rabbitMqPassword;
        _rabbitMqHosts = rabbitMqHosts ?? [];
    }


    private IRabbitMqConnection[]? _rabbitMqConnections;
    private long _currentConnectionIndex;
    private static readonly Lock MakeConnectionLock = new();
    private bool _isInit;

    public void Init(IServiceProvider serviceProvider)
    {
        if (_isInit)
        {
            return;
        }

        try
        {
            lock (MakeConnectionLock)
            {
                if (_isInit)
                {
                    return;
                }

                _isInit = true;
                List<IRabbitMqConnection> rabbitMqConnections = [];
                // var rabbitMqHosts = ConfigSettingEnum.RabbitMqHost.GetConfig().Split('|',
                //     StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (var rabbitMqHost in _rabbitMqHosts)
                {
                    var rabbitMqHostIps = rabbitMqHost.Split(',',
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < _poolSize; i++)
                    {
                        var index = i % _poolSize;
                        var rabbitMqIndex = index % rabbitMqHostIps.Length;
                        string rabbitMqHostIpFirst = rabbitMqHostIps[rabbitMqIndex];
                        List<string> rabbitMqHostIpsConnect = [rabbitMqHostIpFirst];
                        foreach (var rabbitMqHostIp in rabbitMqHostIps)
                        {
                            if (!rabbitMqHostIpsConnect.Contains(rabbitMqHostIp))
                            {
                                rabbitMqHostIpsConnect.Add(rabbitMqHostIp);
                            }
                        }

                        // var virtualHost = ConfigSettingEnum.VirtualHost.GetConfig();
                        ConnectionFactory factory;
                        if (string.IsNullOrEmpty(_virtualHost))
                        {
                            factory = new ConnectionFactory()
                            {
                                HostName = "",
                                UserName = _rabbitMqUserName,
                                Password = _rabbitMqPassword,
                            };
                        }
                        else
                        {
                            factory = new ConnectionFactory()
                            {
                                HostName = "",
                                UserName = _rabbitMqUserName,
                                Password = _rabbitMqPassword,
                                VirtualHost = _virtualHost
                            };
                        }

                        ILogger<DefaultRabbitMqPersistentConnection> logger1 =
                            serviceProvider.GetRequiredService<ILogger<DefaultRabbitMqPersistentConnection>>();
                        IRabbitMqPersistentConnection rabbitMqPersistentConnection =
                            new DefaultRabbitMqPersistentConnection(factory, logger1, rabbitMqHostIpsConnect.ToArray());
                        ILogger<RabbitMqConnection> loggerRabbitMqConnection =
                            serviceProvider.GetRequiredService<ILogger<RabbitMqConnection>>();

                        var rabbitMqConnection =
                            new RabbitMqConnection(rabbitMqPersistentConnection, loggerRabbitMqConnection);
                        rabbitMqConnections.Add(rabbitMqConnection);
                    }
                }

                _rabbitMqConnections = rabbitMqConnections.ToArray();
                _currentConnectionIndex = 0;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);
            throw;
        }
    }

    private int GetCurrentConnectionIndex()
    {
        long c = Interlocked.Increment(ref _currentConnectionIndex);
        var index = c % _poolSize;
        return (int)index;
    }

    public IRabbitMqConnection GetCurrentConnection()
    {
        int currentConnectionIndex = GetCurrentConnectionIndex();
        return _rabbitMqConnections![currentConnectionIndex];
    }

    public async Task RegisterExchange((string ExChange, string Type)[] exchanges)
    {
        if (exchanges is not { Length: > 0 })
        {
            return;
        }

        if (_rabbitMqConnections is not { Length: > 0 })
        {
            return;
        }

        foreach (var rabbitMqConnection in _rabbitMqConnections)
        {
            await rabbitMqConnection.RegisterExchange(exchanges);
        }
    }

    public async Task RegisterExchange(string exChange, string type)
    {
        if (exChange is not { Length: > 0 })
        {
            return;
        }

        if (_rabbitMqConnections is not { Length: > 0 })
        {
            return;
        }

        foreach (var rabbitMqConnection in _rabbitMqConnections)
        {
            _logger.LogInformation("RegisterExchange {RabbitMqConnection} {ExChange} {Type}",
                rabbitMqConnection.GetHosts(), exChange, type);
            await rabbitMqConnection.RegisterExchange(exChange, type);
        }
    }

    public async Task RegisterQueue(string exchange, (string Queue, string Routing)[] queues)
    {
        if (queues is not { Length: > 0 })
        {
            return;
        }

        if (_rabbitMqConnections is not { Length: > 0 })
        {
            return;
        }

        foreach (var rabbitMqConnection in _rabbitMqConnections)
        {
            await rabbitMqConnection.RegisterQueue(exchange, queues);
        }
    }

    public async Task SubscribeQueueAsync(string[] queues, Func<RabbitMqEventBusMessage, Task> processFunc)
    {
        if (queues is not { Length: > 0 })
        {
            return;
        }

        if (_rabbitMqConnections is not { Length: > 0 })
        {
            return;
        }

        foreach (var rabbitMqConnection in _rabbitMqConnections)
        {
            await rabbitMqConnection.SubscribeAsync(queues, processFunc);
        }
    }

    public async Task SubscribeQueueAsync(string[] queues, Func<byte[], Task> processFunc)
    {
        if (queues is not { Length: > 0 })
        {
            return;
        }

        if (_rabbitMqConnections is not { Length: > 0 })
        {
            return;
        }

        foreach (var rabbitMqConnection in _rabbitMqConnections)
        {
            await rabbitMqConnection.SubscribeAsync(queues, processFunc);
        }
    }

    public async Task SubscribeExchangeAsync(string[] exchangesTrigger, Func<RabbitMqEventBusMessage, Task> processFunc)
    {
        if (exchangesTrigger is not { Length: > 0 })
        {
            return;
        }

        if (_rabbitMqConnections is not { Length: > 0 })
        {
            return;
        }

        foreach (var rabbitMqConnection in _rabbitMqConnections)
        {
            await rabbitMqConnection.SubscribeExchangeAsync(exchangesTrigger, processFunc);
        }
    }

    public async Task<(string, string)[]> Send((string, string) exchange, IRabbitMqEvent[] messages)
    {
        var connection = GetCurrentConnection();
        return await connection.Send(exchange, messages);
    }

    public async Task Notify(string exchange, IRabbitMqEvent message)
    {
        var connection = GetCurrentConnection();
        await connection.Notify(exchange, message);
    }

    public async Task NotifyTrigger(string exchange, IRabbitMqEvent[] messages)
    {
        var connection = GetCurrentConnection();
        await connection.NotifyTrigger(exchange, messages);
    }

    public async Task NotifyTrigger(string exchange, RabbitMqEventBusMessage message)
    {
        var connection = GetCurrentConnection();
        await connection.NotifyTrigger(exchange, message);
    }
}