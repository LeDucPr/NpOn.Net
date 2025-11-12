using CommonMode;
using CommonObject;
using Enums;
using ProtoBuf;

namespace RabbitMqBroker;

[ProtoContract]
public record RabbitMqEventBusMessage
{
    public RabbitMqEventBusMessage()
    {
    }

    public RabbitMqEventBusMessage(string topicName, string messageId, object? body, ESerializeType serializeType,
        string? publisher)
    {
        TopicName = topicName;
        SerializeType = serializeType;
        if (body != null)
        {
            BodyType = body.GetType().AssemblyQualifiedName;
            switch (SerializeType)
            {
                case ESerializeType.Json:
                {
                    JsonBody = JsonConverter.ToJson(body);
                    break;
                }
                case ESerializeType.Protobuf:
                {
                    ProtobufBody = ProtoMode.ProtoBufSerialize(body);
                    break;
                }
                case ESerializeType.Byte:
                {
                    ByteBody = (byte[])body;
                    break;
                }
                default:
                {
                    throw new Exception("Body type invalid");
                }
            }
        }
        else
        {
            throw new Exception("Body type invalid");
        }

        CorrelationId = CommonUtilityMode.GenerateGuid();
        MessageId = messageId;
        CreatedDate = DateTime.UtcNow;
        Publisher = publisher;
    }

    [ProtoMember(1)] public required string MessageId { get; set; }
    [ProtoMember(2)] public TimeSpan? Delay { get; set; }
    [ProtoMember(3)] public TimeSpan? TimeToLive { get; set; }
    [ProtoMember(4)] public required string CorrelationId { get; set; }
    [ProtoMember(5)] public required ESerializeType SerializeType { get; set; }
    [ProtoMember(6)] public required int Version { get; set; }
    [ProtoMember(7)] public string? BodyType { get; set; }
    [ProtoMember(8)] public DateTime CreatedDate { get; set; }
    [ProtoMember(9)] public DateTime? ProcessDate { get; set; }
    [ProtoMember(10)] public required string TopicName { get; set; }
    [ProtoMember(11)] public byte[]? ProtobufBody { get; set; }
    [ProtoMember(12)] public string? JsonBody { get; set; }
    [ProtoMember(13)] public byte[]? ByteBody { get; set; }
    [ProtoMember(14)] public string? EventId { get; set; }
    [ProtoMember(15)] public ESerializeType EventType { get; set; }
    [ProtoMember(16)] public long? ExecuteTime { get; set; }
    [ProtoMember(17)] public ERabbitMqEventStatus? Status { get; set; }
    [ProtoMember(18)] public string? Error { get; set; }
    [ProtoMember(19)] public DateTime? FinishDate { get; set; }
    [ProtoMember(20)] public long? SendTime { get; set; }
    [ProtoMember(21)] public string? Publisher { get; set; }
    [ProtoMember(22)] public string? Consumer { get; set; }
}

public static class EventBusMessageExtensions
{
    public static RabbitMqEventBusMessage EventBusMessageCreate(object body, string topicName, string messageId,
        ESerializeType serializeType, string? publisher)
    {
        var message = new RabbitMqEventBusMessage()
        {
            TopicName = topicName,
            SerializeType = serializeType,
            BodyType = body.GetType().AssemblyQualifiedName ?? string.Empty,
            CorrelationId = Guid.CreateVersion7().ToString("N"),
            MessageId = messageId,
            CreatedDate = DateTime.Now,
            Publisher = publisher,
            Version = 1
        };
        switch (serializeType)
        {
            case ESerializeType.Json:
            {
                message.JsonBody = JsonConverter.ToJson(body);
                break;
            }
            case ESerializeType.Protobuf:
            {
                message.ProtobufBody = ProtoMode.ProtoBufSerialize(body);
                break;
            }
            case ESerializeType.Byte:
            {
                message.ByteBody = (byte[])body;
                break;
            }
            default:
            {
                throw new Exception("Body type invalid");
            }
        }

        return message;
    }

    public static object? EventBusMessageToObj(this RabbitMqEventBusMessage message)
    {
        if (message.BodyType == null)
            return null;

        Type? type = Type.GetType(message.BodyType);
        if (type == null)
            throw new Exception("Body type is null");

        switch (message.SerializeType)
        {
            case ESerializeType.Json:
            {
                return JsonConverter.FromJson(message.JsonBody, type);
            }
            case ESerializeType.Protobuf:
            {
                return ProtoMode.ProtoBufDeserialize(message.ProtobufBody, type);
            }
            case ESerializeType.Byte:
            {
                return message.ByteBody;
            }
            default:
            {
                return null;
            }
        }
    }

    public static byte[] EventBusMessageToBytes(this RabbitMqEventBusMessage message)
    {
        return ProtoMode.ProtoBufSerialize(message);
    }

    public static RabbitMqEventBusMessage EventBusMessageClone(this RabbitMqEventBusMessage message)
    {
        return new RabbitMqEventBusMessage()
        {
            BodyType = message.BodyType,
            CorrelationId = message.CorrelationId,
            CreatedDate = message.CreatedDate,
            Delay = message.Delay,
            MessageId = message.MessageId,
            ProcessDate = message.ProcessDate,
            SerializeType = message.SerializeType,
            TimeToLive = message.TimeToLive,
            Version = message.Version,
            TopicName = message.TopicName,
            ProtobufBody = message.ProtobufBody,
            JsonBody = message.JsonBody,
            ByteBody = message.ByteBody,
            EventId = message.EventId,
            EventType = message.EventType,
            ExecuteTime = message.ExecuteTime,
            Status = message.Status,
            Error = message.Error,
            FinishDate = message.FinishDate,
            SendTime = message.SendTime,
            Publisher = message.Publisher,
            Consumer = message.Consumer
        };
    }

    public static RabbitMqEventBusMessage? CreateMessageFromQueue(byte[]? bytes)
    {
        return ProtoMode.ProtoBufDeserialize<RabbitMqEventBusMessage>(bytes);
    }
}