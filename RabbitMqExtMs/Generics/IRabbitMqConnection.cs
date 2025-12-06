using RabbitMQ.Client;

namespace RabbitMqExtMs.Generics;

public interface IRabbitMqConnection
{
    public IChannel Channel { get; }
    public string RoutingKey { get; }
    public string ExchangeName { get; }

    Task AddDefaultQueue(string exchangeName, string queueName,
        bool isCreateNewExchangeWhenExisted = false, bool isCreateNewQueueWhenExisted = false);

    Task AddQueue(RabbitMqQueueProperty property, bool isCreateNewExchangeWhenExisted = false,
        bool isCreateNewQueueWhenExisted = false);
}