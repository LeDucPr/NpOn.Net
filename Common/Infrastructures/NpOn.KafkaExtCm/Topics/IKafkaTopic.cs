using Confluent.Kafka;

namespace Common.Infrastructures.NpOn.KafkaExtCm.Topics;

public interface IKafkaTopic
{
    public string GetTopicName();
    public ClientConfig GetKafkaConfig();
}