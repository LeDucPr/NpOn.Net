using System.ComponentModel.DataAnnotations;

namespace Common.Extensions.NpOn.CommonEnums;

public enum ERabbitMqExchangeType
{
    [Display(Name = "direct")] Direct,
    [Display(Name = "fanout")] Fanout,
    [Display(Name = "topic")] Topic,
    [Display(Name = "headers")] Headers,
}

public enum ERabbitMqQueuePropertyArgument
{
    [Display(Name = "x-message-ttl")] XMessageTtl, // time to live for message (in milliseconds)
    [Display(Name = "x-dead-letter-exchange")] DeadLetterExchange, // exchange name for dead message
    [Display(Name = "x-dead-letter-routing-key")] DeadLetterRoutingKey, // queue name for dead message
}

public enum ERabbitMqResponseType
{
    [Display(Name = "Default")] Default, 
    [Display(Name = "BasicAck")] BasicAck, 
    [Display(Name = "BasicNack")] BasicNack, 
    [Display(Name = "BasicReject")] BasicReject
}