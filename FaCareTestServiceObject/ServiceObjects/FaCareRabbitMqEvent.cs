using CommonObject.CommonObjects;
using Enums;
using FaCareTestServiceObject.ServiceObjects.RabbitMqEvents;
using ProtoBuf;

namespace FaCareTestServiceObject.ServiceObjects;

[ProtoContract]
[ProtoInclude(100, typeof(RabbitMqTestEvent))]
public record FaCareRabbitMqEvent : IRabbitMqEvent
{
    [ProtoMember(1)] public string EventId { get; set; }
    [ProtoMember(2)] public int Version { get; set; }
    [ProtoMember(3)] public ESerializeType EventType { get; }
    [ProtoMember(4)] public bool IsTrigger { get; set; }
    [ProtoMember(5)] public ESerializeType SerializeType { get; set; }
    [ProtoMember(6)] public string? Publisher { get; set; }
}