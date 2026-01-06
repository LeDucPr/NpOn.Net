namespace Common.Infrastructures.NpOn.KafkaExtCm.Events;

public interface IKafkaEvent
{
    Guid MessageId { get; set; }
    string? StringContent { get; set; }
    string? EventType { get; set; }
    DateTime Timestamp { get; set; }
    Dictionary<string, string>? Headers { get; set; }
}