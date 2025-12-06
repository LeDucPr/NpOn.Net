using CommonMode;
using RabbitMQ.Client;
using RabbitMqExtMs.Events;
using RabbitMqExtMs.Generics;

namespace RabbitMqExtMs.Senders;

public class RabbitMqProducer
{
    private readonly IRabbitMqConnection _rabbitMqConnection;

    public RabbitMqProducer(IRabbitMqConnection rabbitMqConnection)
    {
        _rabbitMqConnection = rabbitMqConnection;
    }

    public RabbitMqProducer(string connectionString)
    {
        _rabbitMqConnection = new RabbitMqConnection(connectionString);
    }

    public async Task AddEvent(IRabbitMqEvent @event, bool isCompress = false)
    {
        var eventType = @event.GetType();
        if (!eventType.IsGenericType || eventType.GetGenericTypeDefinition() != typeof(RabbitMqEvent<>))
            return;

        // Dynamically get the ExchangeName and RoutingKey based on the event's generic type.
        var messageContentType = eventType.GetGenericArguments()[0];
        var componentType = typeof(RabbitMqComponent<>).MakeGenericType(messageContentType);
        dynamic component = Activator.CreateInstance(componentType)!; // ??

        string exchangeName = component.ExchangeName;
        string queueName = component.QueueName;
        string routingKey = component.RoutingKey;
        await _rabbitMqConnection.AddDefaultQueue(exchangeName, queueName);

        var body = ProtoMode.ProtoBufSerialize(@event, isCompress);
        var props = new BasicProperties { Persistent = true };

        await _rabbitMqConnection.Channel.BasicPublishAsync<BasicProperties>(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: props,
            body: body);
    }
}