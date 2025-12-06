using RabbitMqExtMs.Events;

namespace RabbitMqExtMs.Generics;

/// <summary>
/// Simply put, it is a message information sharing system between sender and receiver
/// </summary>
/// <typeparam name="T"></typeparam>
public class RabbitMqComponent<T> where T : RabbitMqMessageContent
{
    public RabbitMqComponent()
    {
        TypeT ??= typeof(T);
        string? typeTFullname = TypeT.FullName;
        var parts = typeTFullname?.Split('.');
        IsEnableType = parts is { Length: >= 2 };
#if DEBUG
        string lastTwo = $"{parts[^2]}.{parts[^1]}";
#endif
        ExchangeName = parts[^2];
        QueueName = parts[^1];
        RoutingKey = $"{ExchangeName}.{QueueName}";
    }

    public Type TypeT { get; init; }
    public string? HashType => TypeT.FullName?.GetHashCode().ToString();
    public bool IsEnableType { get; }
    public string ExchangeName { get; }
    public string QueueName { get; }
    public string RoutingKey { get; }
}