﻿using ProtoBuf;

namespace RabbitMqExtMs.Events;

[ProtoContract]
// Tell protobuf-net that TestEvent is a known subtype of RabbitMqMessageContent.
// The number '100' is an arbitrary but unique tag for this subtype within the hierarchy.
// If you have more subtypes, add more [ProtoInclude] attributes with unique tags.
[ProtoInclude(100, typeof(TestEvent))]
public abstract class RabbitMqMessageContent
{
}