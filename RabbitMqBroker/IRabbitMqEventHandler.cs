using CommonObject.CommonObjects;

namespace RabbitMqBroker;

public interface IRabbitMqMessageHandler<in TI> : IRabbitMqMessageHandler where TI : IRabbitMqEvent
{
    Task Handle(TI message, string topic);
}