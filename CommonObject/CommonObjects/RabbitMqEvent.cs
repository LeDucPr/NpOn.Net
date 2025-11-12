using Enums;

namespace CommonObject.CommonObjects;

public interface IRabbitMqEvent
{
    string EventId { get; set; }
    int Version { get; set; }
    ESerializeType EventType { get; }
    public bool IsTrigger { get; set; }
    public ESerializeType SerializeType { get; set; }
    public string? Publisher { get; set; }
}

public abstract class RabbitMqEvent : IRabbitMqEvent
{
    private ESerializeType _serializeType;

    protected RabbitMqEvent()
    {
        EventId = Guid.CreateVersion7().ToString("N");
        DelayTime = 0;
        Version = 0;
        ProcessDate = DateTime.Now;
    }

    public abstract string EventId { get; set; }
    public abstract int DelayTime { get; set; }
    public abstract int Version { get; set; }
    public ESerializeType EventType { get; }
    public abstract bool IsTrigger { get; set; }

    public abstract string? ObjectId { get; set; }
    public abstract string? ProcessUid { get; set; }
    public abstract DateTime ProcessDate { get; set; }
    public DateTime ProcessDateUtc => ProcessDate.ToUniversalTime();
    public abstract string? LoginUid { get; set; }
    public ESerializeType SerializeType { get; set; }
    public string? Publisher { get; set; }

}