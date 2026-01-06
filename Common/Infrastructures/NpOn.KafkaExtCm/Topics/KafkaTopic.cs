using Common.Infrastructures.NpOn.KafkaExtCm.Configs;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Common.Infrastructures.NpOn.KafkaExtCm.Topics;

public class KafkaTopic(
    ClientConfig config,
    int partitions = 1,
    short replicationFactor = 1,
    Dictionary<int, List<int>>? replicasAssignments = null
) : IKafkaTopic
{
    private string _topicName = string.Empty;
    public string GetTopicName() => _topicName;
    public ClientConfig GetKafkaConfig() => config;

    public static async Task<KafkaTopic> Create(
        ClientConfig config,
        string topicName,
        int partitions = 1,
        short replicationFactor = 1,
        Dictionary<int, List<int>>? replicasAssignments = null /* manual fix (I dont know) */)
    {
        var topicInstance = new KafkaTopic(config, partitions, replicationFactor, replicasAssignments);
        await topicInstance.CreateTopicAsync(topicName);
        return topicInstance;
    }

    private async Task CreateTopicAsync(string topicName)
    {
        _topicName = topicName;
        using var adminClient = new AdminClientBuilder(config).Build();
        try
        {
            var configs = KafkaDefaultConfig.TopicConfigs.GetAllConfigAsString();

            if (replicasAssignments is { Count: > 0 })
            {
                await adminClient.CreateTopicsAsync([
                    new TopicSpecification
                    {
                        Name = topicName,
                        NumPartitions = -1,
                        ReplicationFactor = replicationFactor,
                        ReplicasAssignments = replicasAssignments,
                        Configs = configs
                    }
                ]);
                return;
            }

            await adminClient.CreateTopicsAsync([
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = partitions,
                    ReplicationFactor = replicationFactor,
                    Configs = configs
                }
            ]);
        }
        catch (CreateTopicsException)
        {
            // logger.LogWarning($"[Info] Topic creation check: {e.Results[0].Error.Reason}");
        }
    }
}