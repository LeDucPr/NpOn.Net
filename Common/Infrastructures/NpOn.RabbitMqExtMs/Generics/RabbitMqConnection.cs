using Common.Extensions.NpOn.CommonMode;
using RabbitMQ.Client;

namespace Common.Infrastructures.NpOn.RabbitMqExtMs.Generics;

public class RabbitMqConnection : IRabbitMqConnection, IDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private IConnection _connection;
    private IChannel _channel;
    private Dictionary<string, RabbitMqQueueProperty> _queueProperties;
    private string _routingKey = string.Empty;
    private string _exchangeName = string.Empty;
    private bool _disposed;

    public RabbitMqConnection(string connectString, string exchangeName)
    {
        _connectionFactory = new ConnectionFactory()
        {
            Uri = new Uri(connectString) // amqp://rabbitmq:password@localhost:5672/
        };
        CreateConnection().GetAwaiter().GetResult();
        _exchangeName = exchangeName;
        _queueProperties = new Dictionary<string, RabbitMqQueueProperty>();
    }

    private async Task CreateConnection()
    {
        _connection = await _connectionFactory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public IChannel Channel => _channel;
    public string RoutingKey => _routingKey;
    public string ExchangeName => _exchangeName;

    public async Task<string> AddDefaultQueue(string exchangeName, string queueName,
        bool isCreateNewExchangeWhenExisted = false, bool isCreateNewQueueWhenExisted = false)
    {
        RabbitMqQueueProperty newQueueProperty = new RabbitMqQueueProperty
        {
            ExchangeName = exchangeName,
            QueueName = queueName,
        };
        
        var routingKey = newQueueProperty.RoutingKey; // routingKey is queueName
        _exchangeName = exchangeName;
        // Logic was inverted. We should declare the queue if it does NOT exist.
        if (!_queueProperties.ContainsKey(routingKey))
        {
            // Declare the exchange
            await _channel.ExchangeDeclareAsync(
                exchange: newQueueProperty.ExchangeName,
                type: newQueueProperty.ExchangeType.GetDisplayName(),
                durable: newQueueProperty.Durable,
                autoDelete: newQueueProperty.AutoDelete,
                arguments: newQueueProperty.DictArgument);

            // Declare the queue
            await _channel.QueueDeclareAsync(queue: routingKey,
                durable: newQueueProperty.Durable,
                exclusive: newQueueProperty.Exclusive,
                autoDelete: newQueueProperty.AutoDelete,
                arguments: newQueueProperty.DictArgument);

            // Bind the queue to the exchange
            await _channel.QueueBindAsync(
                queue: routingKey,
                exchange: newQueueProperty.ExchangeName,
                routingKey: routingKey);

            _queueProperties.Add(routingKey, newQueueProperty);
        }

        return routingKey;
    }

    public async Task AddQueue(RabbitMqQueueProperty property, bool isCreateNewExchangeWhenExisted = false,
        bool isCreateNewQueueWhenExisted = false)
    {
        _exchangeName = property.ExchangeName;
        _routingKey = property.RoutingKey;
        if (_queueProperties.ContainsKey(_routingKey))
        {
            if (isCreateNewExchangeWhenExisted)
            {
                await _channel.ExchangeDeleteAsync(property.ExchangeName);
                await _channel.ExchangeDeclareAsync(
                    exchange: property.ExchangeName,
                    type: property.ExchangeType.GetDisplayName(),
                    durable: property.Durable,
                    autoDelete: property.AutoDelete,
                    arguments: property.DictArgument);
            }

            if (isCreateNewQueueWhenExisted)
            {
                await _channel.QueueDeclareAsync(queue: _routingKey,
                    durable: property.Durable,
                    exclusive: property.Exclusive,
                    autoDelete: property.AutoDelete,
                    arguments: property.DictArgument);
            }

            _queueProperties.Remove(_routingKey);
            _queueProperties.Add(_routingKey, property);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            try
            {
                _channel?.CloseAsync();
                _connection?.CloseAsync();
            }
            catch (Exception)
            {
                /* disposed */
            }
        }

        _disposed = true;
    }
}