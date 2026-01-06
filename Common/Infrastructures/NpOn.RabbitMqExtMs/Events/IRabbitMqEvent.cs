namespace Common.Infrastructures.NpOn.RabbitMqExtMs.Events;

public interface IRabbitMqEvent
{
    Guid MessageId { get; set; }
    string? StringContent { get; set; }
    string? EventType { get; set; }
    DateTime Timestamp { get; set; }
    Dictionary<string, string>? Headers { get; set; }
}