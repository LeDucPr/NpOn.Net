using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.RabbitMqExtMs.Events;
using Common.Infrastructures.NpOn.RabbitMqExtMs.Generics;
using RabbitMQ.Client;

namespace Common.Infrastructures.NpOn.RabbitMqExtMs.Senders;

public class RabbitMqProducer(IRabbitMqConnection rabbitMqConnection) : IRabbitMqProducer
{
    public void AddEvent(IRabbitMqEvent @event, bool isCompress = false)
    {
        FireAndForget(() => PublishAsync(@event, isCompress));
    }

    private async Task PublishAsync(IRabbitMqEvent @event, bool isCompress)
    {
        var eventType = @event.GetType();
        if (!eventType.IsGenericType ||
            eventType.GetGenericTypeDefinition() != typeof(RabbitMqEvent<>))
            return;

        var messageContentType = eventType.GetGenericArguments()[0];
        var componentType = typeof(RabbitMqComponent<>).MakeGenericType(messageContentType);
        dynamic component = Activator.CreateInstance(componentType)!;

        string queueName = component.QueueName;
        string routingKey = component.RoutingKey;

        string exchangeName = rabbitMqConnection.ExchangeName;
        await rabbitMqConnection.AddDefaultQueue(exchangeName, queueName);

        var body = ProtoBufMode.ProtoBufSerialize(@event, isCompress);
        var props = new BasicProperties { Persistent = true };

        await rabbitMqConnection.Channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: props,
            body: body);
    }

    private void FireAndForget(Func<Task> task)
    {
        _ = Task.Run(async () => { await task(); });
    }
}