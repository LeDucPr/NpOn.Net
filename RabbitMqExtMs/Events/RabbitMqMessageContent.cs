using ProtoBuf;

namespace RabbitMqExtMs.Events;

[ProtoContract]
// [ProtoInclude(100, typeof(TestEvent))]
public abstract class RabbitMqMessageContent
{
}