using RabbitMqExtMs.Events;

namespace RabbitMqExtMs.Senders;

public interface IRabbitMqProducer
{
    Task AddEvent(IRabbitMqEvent @event, bool isCompress = false);
}