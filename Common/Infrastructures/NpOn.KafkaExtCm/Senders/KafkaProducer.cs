using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.KafkaExtCm.Events;
using Common.Infrastructures.NpOn.KafkaExtCm.Generics;
using Common.Infrastructures.NpOn.KafkaExtCm.Topics;
using Confluent.Kafka;

namespace Common.Infrastructures.NpOn.KafkaExtCm.Senders;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IKafkaTopic _kafkaTopic;
    private readonly IProducer<string, byte[]> _producer;

    public KafkaProducer(IKafkaTopic kafkaTopic)
    {
        _kafkaTopic = kafkaTopic;
        var config = new ProducerConfig(kafkaTopic.GetKafkaConfig());
        _producer = new ProducerBuilder<string, byte[]>(config).Build();
    }

    public void AddEvent(IKafkaEvent @event, bool isCompress = false)
    {
        FireAndForget(() => PublishAsync(@event, isCompress));
    }

    private async Task PublishAsync(IKafkaEvent @event, bool isCompress)
    {
        var eventType = @event.GetType();
        if (!eventType.IsGenericType ||
            eventType.GetGenericTypeDefinition() != typeof(KafkaEvent<>))
            return;

        var messageContentType = eventType.GetGenericArguments()[0];
        var componentType = typeof(KafkaComponent<>).MakeGenericType(messageContentType);
        dynamic component = Activator.CreateInstance(componentType)!;

        string topicName = _kafkaTopic.GetTopicName();
        // string partitionName = component.PartitionName;
        string routingKey = component.RoutingKey;

        var body = ProtoBufMode.ProtoBufSerialize(@event, isCompress);
        await _producer.ProduceAsync(
            topicName,
            new Message<string, byte[]> { Key = routingKey, Value = body }
        );
    }

    private void FireAndForget(Func<Task> task)
    {
        _ = Task.Run(async () => { await task(); });
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}