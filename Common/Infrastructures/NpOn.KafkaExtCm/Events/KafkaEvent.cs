using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;
using ProtoBuf;

namespace Common.Infrastructures.NpOn.KafkaExtCm.Events;

[ProtoContract]
public class KafkaEvent<T> : IKafkaEvent where T : CommonMessageContent
{
    [ProtoMember(1)] public Guid MessageId { get; set; } = CommonUtilityMode.GenerateGuid();
    [ProtoMember(2)] public string? StringContent { get; set; }
    [ProtoMember(3)] public string? EventType { get; set; }
    [ProtoMember(4)] public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    [ProtoMember(5)] public Dictionary<string, string>? Headers { get; set; }
    [ProtoMember(6)] public T? MessageContent { get; set; }
}