using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.RabbitMqExtMs.Events;
using Common.Infrastructures.NpOn.RabbitMqExtMs.Generics;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Common.Infrastructures.NpOn.RabbitMqExtMs.Receivers;

public abstract class RabbitMqConsumer<T> : RabbitMqComponent<T>, IRabbitMqConsumer<T>, IDisposable
    where T : CommonMessageContent
{
    private readonly ILogger<RabbitMqConsumer<T>>? _logger;
    private readonly IRabbitMqConnection _rabbitMqConnection;
    private Type _typeT;
    private ERabbitMqResponseType _responseType = ERabbitMqResponseType.BasicAck;
    protected Func<T, Task>? Handler;
    private readonly bool _autoAck; // = true;
    private readonly bool _isExternalConnection;

    public ERabbitMqResponseType ResponseType
    {
        get => _responseType;
        set => _responseType = value;
    }

    public RabbitMqConsumer(IRabbitMqConnection rabbitMqConnection, bool autoAck = true,
        ILogger<RabbitMqConsumer<T>>? logger = null) // : base()
    {
        _rabbitMqConnection = rabbitMqConnection;
        _isExternalConnection = true; // Mark connection as external, do not dispose it
        _typeT = typeof(T);
        // _handler = handler;
        _logger = logger;
        _autoAck = autoAck;

        // Call the abstract method to ensure the handler is set by the derived class *before* listening starts.
        AddHandler();
        UseDefault().GetAwaiter().GetResult();
    }

    // public RabbitMqConsumer(string connectionString, Func<T, Task> handler, bool autoAck = true) // : base()
    // {
    //     _rabbitMqConnection = new RabbitMqConnection(connectionString);
    //     _typeT = typeof(T);
    //     _handler = handler;
    //     _autoAck = autoAck;
    // }

    public async Task UseDefault(bool isDecompress = false)
    {
        if (!IsEnableType)
            return;
        var routingKey = await _rabbitMqConnection.AddDefaultQueue(_rabbitMqConnection.ExchangeName/* ?? ExchangeName*/, QueueName);
        IChannel channel = _rabbitMqConnection.Channel;
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            var fullEvent = ProtoBufMode.ProtoBufDeserialize<RabbitMqEvent<T>>(body, isDecompress);
            if (Handler != null && fullEvent?.MessageContent != null)
            {
                if (_autoAck)
                {
                    try
                    {
                        await Handler(fullEvent.MessageContent);
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false); // ack when done (clear)
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Handler error: {ex.Message}");
                        // nack when error and retry
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                    }
                }
                else
                {
                    _ = Task.Run(() => Handler(fullEvent.MessageContent)); // run task in background
                    switch (_responseType)
                    {
                        case ERabbitMqResponseType.BasicAck:
                            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                            break;
                        case ERabbitMqResponseType.BasicNack:
                            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                            break;
                        case ERabbitMqResponseType.BasicReject:
                            await channel.BasicRejectAsync(ea.DeliveryTag, requeue: true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        };

        await channel.BasicConsumeAsync(
            queue: routingKey,
            autoAck: _responseType == ERabbitMqResponseType.Default, // false when use switch - case {_responseType}
            consumer: consumer);
    }

    public void Dispose()
    {
        // Only dispose the connection if this class created it.
        // If the connection was passed in from the outside (Dependency Injection),
        // the outside code is responsible for its lifetime.
        if (!_isExternalConnection && _rabbitMqConnection is IDisposable disposableConnection)
        {
            disposableConnection.Dispose();
        }
    }

    public abstract void AddHandler();
}