namespace RabbitMqBroker;

public interface IRabbitMqEventProcessor
{
    void Register();
    Dictionary<string, string> Handle(RabbitMqEventBusMessage payload);
    Task Start();
}