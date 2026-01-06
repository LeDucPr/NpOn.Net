using Common.Infrastructures.NpOn.KafkaExtCm.Events;

namespace Common.Infrastructures.NpOn.KafkaExtCm.Senders;

public interface IKafkaProducer
{
    void AddEvent(IKafkaEvent @event, bool isCompress = false);
}