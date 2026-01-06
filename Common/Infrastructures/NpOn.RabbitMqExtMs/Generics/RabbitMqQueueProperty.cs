using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;

namespace Common.Infrastructures.NpOn.RabbitMqExtMs.Generics;

public class RabbitMqQueueProperty
{
    public required string ExchangeName { get; set; }
    public ERabbitMqExchangeType ExchangeType { get; set; } = ERabbitMqExchangeType.Direct;

    public required string QueueName { get; set; } // queue name 
    public string RoutingKey => $"{ExchangeName}.{QueueName}";
    public bool Durable { get; set; } = true; // on disk 
    public bool AutoDelete { get; set; } = false; // auto delete when not has any consumers connect to 
    public bool Exclusive { get; set; } = false; // Only Use by Creator
    public RabbitMqQueuePropertyArgument[]? Argument { get; set; }
    public Dictionary<string, object?>? DictArgument => Argument?.ToDictionary(x => x.PropertyKey!, x => x.Value);
}

public class RabbitMqQueuePropertyArgument
{
    public required ERabbitMqQueuePropertyArgument Property { get; set; }
    public string? PropertyKey => Property.GetDisplayName();
    public Object? Value { get; set; } // int ? string
}