using ProtoBuf;

namespace FaCareTestServiceObject.ServiceObjects.RabbitMqEvents;

[ProtoContract]
public record RabbitMqTestEvent : FaCareRabbitMqEvent
{
    [ProtoMember(1)] public required string KuIsHss { get; set; }
}