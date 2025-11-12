using Microsoft.Extensions.Logging;

namespace RabbitMqBroker;

public class RabbitMqConnection(
    IRabbitMqPersistentConnection persistentConnection,
    ILogger<RabbitMqConnection> logger)
    : RabbitMqBaseConnection(persistentConnection, logger, RabbitMqPrefetchCount), IRabbitMqConnection
{
    private static readonly int RabbitMqPrefetchCount = 10; // ??
}