using Common.Infrastructures.NpOn.RabbitMqExtMs.Events;

namespace Common.Infrastructures.NpOn.RabbitMqExtMs.Senders;

public interface IRabbitMqProducer
{
    void AddEvent(IRabbitMqEvent @event, bool isCompress = false);
}