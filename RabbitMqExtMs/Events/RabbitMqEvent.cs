using CommonMode;
using ProtoBuf;

namespace RabbitMqExtMs.Events;

[ProtoContract]
public class RabbitMqEvent<T> : IRabbitMqEvent where T : RabbitMqMessageContent
{
    [ProtoMember(1)] public Guid MessageId { get; set; } = CommonUtilityMode.GenerateGuid();
    [ProtoMember(2)] public string? StringContent { get; set; }
    [ProtoMember(3)] public string? EventType { get; set; }
    [ProtoMember(4)] public DateTime Timestamp { get; set; }
    [ProtoMember(5)] public Dictionary<string, string>? Headers { get; set; }
    [ProtoMember(6)] public T? MessageContent { get; set; }
}