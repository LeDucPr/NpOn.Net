using RabbitMQ.Client;

namespace RabbitMqBroker;

public interface IRabbitMqPersistentConnection : IDisposable
{
    bool IsConnected { get; }
    Task<bool> TryConnect();
    Task<IChannel> CreateChannel();
    string GetHosts();
}