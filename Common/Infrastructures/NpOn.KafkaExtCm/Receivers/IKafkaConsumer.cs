using Common.Extensions.NpOn.CommonGrpcContract;

namespace Common.Infrastructures.NpOn.KafkaExtCm.Receivers;

public interface IKafkaConsumer
{
}

public interface IKafkaConsumer<T> : IKafkaConsumer where T : CommonMessageContent
{
    public abstract void AddHandler();
}