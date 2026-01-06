using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;

namespace Common.Infrastructures.NpOn.KafkaExtCm.Generics;

/// <summary>
/// Simply put, it is a message information sharing system between sender and receiver
/// </summary>
/// <typeparam name="T"></typeparam>
public class KafkaComponent<T> where T : CommonMessageContent
{
    public KafkaComponent()
    {
        TypeT ??= typeof(T);
        string? typeTFullname = TypeT.FullName;
        var parts = typeTFullname?.Split('.');
        IsEnableType = parts is { Length: >= 2 };
#if DEBUG
        _ = $"{parts![^2]}.{parts[^1]}";
#endif
        TopicName = EApplicationConfiguration.KafkaTopicName.GetAppSettingConfig() ?? parts[^2];
        PartitionName = parts[^1];
        RoutingKey = $"{TopicName}.{PartitionName}";
    }

    public Type TypeT { get; init; }
    public string? HashType => TypeT.FullName?.GetHashCode().ToString();
    public bool IsEnableType { get; }
    public string TopicName { get; }
    public string PartitionName { get; }
    public string RoutingKey { get; }
}