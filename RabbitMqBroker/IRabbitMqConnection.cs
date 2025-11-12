using CommonObject.CommonObjects;
using RabbitMQ.Client;

namespace RabbitMqBroker;

public interface IRabbitMqConnection
{
    Task Notify(string exchange, IRabbitMqEvent message);
    Task<(string, string)[]> Send((string, string) exchange, IRabbitMqEvent[] messages);
    Task<string> Send((string, string) topic, byte[] message);
    Task Send((string, string) topic, byte[][] messages);
    Task NotifyTrigger(string exchange, IRabbitMqEvent[] messages);
    Task NotifyTrigger(string exchange, RabbitMqEventBusMessage message);
    Task SubscribeAsync(string[] topics, Func<RabbitMqEventBusMessage, Task> processFunc);
    Task SubscribeAsync(string[] topics, Func<byte[], Task> processFunc);
    Task SubscribeNotifyAsync(string[] exchangesTrigger, Func<RabbitMqEventBusMessage, Task> processFunc);
    Task SubscribeExchangeAsync(string[] exchangesTrigger, Func<RabbitMqEventBusMessage, Task> processFunc);
    Task RegisterExchangeAndQueue(string exchange, string[] routingKeys, string[] queues);
    Task RegisterExchangeTrigger(string[] exchanges);
    Task<(IChannel? Channel, string? ConsumerTag )> SubscribeAsync(string queue, Func<byte[], Task> processFunc,
        int? rabbitMqPrefetchCount);
    Task RegisterExchange((string ExChange, string Type)[] exchanges);
    Task RegisterExchange(string exChange, string type);
    Task RegisterQueue(string exchange, (string Queue, string Routing)[] queues);
    string GetHosts();
    string Hosts { get; }
}