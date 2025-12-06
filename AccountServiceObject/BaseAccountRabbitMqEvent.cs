using AccountServiceObject.EventObjects;
using ProtoBuf;
using RabbitMqExtMs.Events;

namespace AccountServiceObject;

[ProtoContract]
[ProtoInclude(100, typeof(AccountSaveLoginEvent))]
public abstract class BaseAccountRabbitMqEvent : RabbitMqMessageContent
{
}