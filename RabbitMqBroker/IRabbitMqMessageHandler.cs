namespace RabbitMqBroker;

public interface IRabbitMqMessageHandler
{
    public string WorkerGroup { get; }
}