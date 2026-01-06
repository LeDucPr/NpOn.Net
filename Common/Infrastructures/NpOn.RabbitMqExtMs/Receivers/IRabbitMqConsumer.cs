using Common.Extensions.NpOn.CommonGrpcContract;

namespace Common.Infrastructures.NpOn.RabbitMqExtMs.Receivers;

public interface IRabbitMqConsumer
{
}

public interface IRabbitMqConsumer<T> : IRabbitMqConsumer where T : CommonMessageContent
{
    public abstract void AddHandler();
}