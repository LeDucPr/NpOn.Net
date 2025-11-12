using System.Net.Sockets;
using System.Text;
using CommonMode;
using CommonObject;
using CommonObject.CommonObjects;
using Enums;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace RabbitMqBroker;

public class RabbitMqBaseConnection(
    IRabbitMqPersistentConnection persistentConnection,
    ILogger logger,
    int? rabbitMqPrefetchCount)
    : IRabbitMqConnection
{
    private readonly ObjectPool _objectPool = new(persistentConnection, logger);
    private int? _rabbitMqPrefetchCount = rabbitMqPrefetchCount;

    public async Task Notify(string exchange, IRabbitMqEvent message)
    {
        var policy = Policy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) => { logger.LogWarning("{Message}", ex.ToString()); });
        var channel = await _objectPool.Get();
        if (channel == null)
        {
            throw new Exception("channel is null");
        }

        try
        {
            string exchangeNotify = $"{exchange}".ToLower();
            var props = new BasicProperties();
            var msg = EventBusMessageExtensions.EventBusMessageCreate(message, exchange, message.EventId,
                message.SerializeType == ESerializeType.Json
                    ? ESerializeType.Json
                    : ESerializeType.Protobuf, message.Publisher);
            props.ContentType = msg.BodyType;
            await policy.Execute(async () =>
            {
                try
                {
                    logger.LogInformation("exchangeNotify: {ExchangeNotify}", exchangeNotify);
                    logger.LogInformation("exchangeNotify EventType: {EventType}", message.EventType);
                    byte[] body;
                    if (message.SerializeType == ESerializeType.Json)
                    {
                        string json = JsonConverter.ToJson(msg);
                        body = Encoding.UTF8.GetBytes(json);
                    }
                    else
                    {
                        body = ProtoMode.ProtoBufSerialize(msg);
                    }

                    await channel.BasicPublishAsync(exchangeNotify, "", mandatory: false, basicProperties: props,
                        body: body);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "{Message}", e.Message);
                    throw;
                }
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
            throw;
        }
        finally
        {
            _objectPool.Return(channel);
        }
    }

    public async Task<(string, string)[]> Send((string, string) exchange, IRabbitMqEvent[] messages)
    {
        var policy = Policy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) => { logger.LogWarning("{Message}", ex.ToString()); });
        (string, string)[] results = new (string, string)[messages.Length];
        var channel = await _objectPool.Get();
        if (channel == null)
        {
            throw new Exception("channel is null");
        }

        try
        {
            int i = 0;
            foreach (var message in messages)
            {
                results[i] = (message.EventId, string.Empty);
                try
                {
                    RabbitMqEventBusMessage msg = EventBusMessageExtensions.EventBusMessageCreate(message,
                        exchange.Item1,
                        message.EventId,
                        message.SerializeType == ESerializeType.Json
                            ? ESerializeType.Json
                            : ESerializeType.Protobuf, message.Publisher);
                    await policy.Execute(async () =>
                    {
                        try
                        {
                            var props = new BasicProperties();
                            props.Headers ??= new Dictionary<string, object?>();
                            props.ContentType = msg.BodyType;
                            string exChange = $"{exchange.Item1}".ToLower();
                            string routingKey = $"{exchange.Item2}.{message.EventType}".ToLower();
                            byte[] body;
                            if (message.SerializeType == ESerializeType.Json)
                            {
                                string json = JsonConverter.ToJson(msg);
                                body = Encoding.UTF8.GetBytes(json);
                            }
                            else
                            {
                                body = ProtoMode.ProtoBufSerialize(msg);
                            }

                            logger.LogInformation("ExChange = {ExChange}", exChange);
                            logger.LogInformation("RoutingKey = {RoutingKey}", routingKey);
                            await channel.BasicPublishAsync(exChange, routingKey, false, props, body);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "{Message}", e.Message);
                            throw;
                        }
                    });
                }
                catch (Exception e)
                {
                    results[i].Item2 = e.Message;
                    logger.LogError(e, "{Message}", e.Message);
                }

                i++;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
            throw;
        }
        finally
        {
            _objectPool.Return(channel);
        }

        return results;
    }

    public async Task<string> Send((string, string) topic, byte[] message)
    {
        string error = string.Empty;
        // if (!_persistentConnection.IsConnected)
        // {
        //     _persistentConnection.TryConnect();
        // }
        var policy = Policy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) => { logger.LogWarning("{Message}", ex.ToString()); });
        var channel = await _objectPool.Get();
        if (channel == null)
        {
            throw new Exception("channel is null");
        }

        try
        {
            await policy.Execute(async () =>
            {
                try
                {
                    var props = new BasicProperties();
                    props.Headers ??= new Dictionary<string, object?>();
                    props.ContentType = typeof(byte[]).FullName;
                    string exChange = $"{topic.Item1}".ToLower();
                    string routingKey = $"{topic.Item2}".ToLower();
                    await channel.BasicPublishAsync(exChange, routingKey, false, props, message);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "{Message}", e.Message);
                    throw;
                }
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
            error = e.Message;
        }
        finally
        {
            _objectPool.Return(channel);
        }

        return error;
    }

    public async Task Send((string, string) topic, byte[][] messages)
    {
        var policy = Policy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) => { logger.LogWarning("{Message}", ex.ToString()); });
        var channel = await _objectPool.Get();
        if (channel == null)
        {
            throw new Exception("channel is null");
        }

        try
        {
            foreach (var message in messages)
            {
                try
                {
                    await policy.Execute(async () =>
                    {
                        try
                        {
                            var props = new BasicProperties();
                            props.Headers ??= new Dictionary<string, object?>();
                            props.ContentType = typeof(byte[]).FullName;
                            string exChange = $"{topic.Item1}".ToLower();
                            string routingKey = $"{topic.Item2}".ToLower();
                            logger.LogInformation("exChange = {ExChange}", exChange);
                            logger.LogInformation("routingKey = {RoutingKey}", routingKey);
                            await channel.BasicPublishAsync(exChange, routingKey, false, props, message);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "{Message}", e.Message);
                            throw;
                        }
                    });
                }
                catch (Exception e)
                {
                    logger.LogError(e, "{Message}", e.Message);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
            throw;
        }
        finally
        {
            _objectPool.Return(channel);
        }
    }

    public async Task NotifyTrigger(string exchange, IRabbitMqEvent[] messages)
    {
        // if (!_persistentConnection.IsConnected)
        // {
        //     _persistentConnection.TryConnect();
        // }

        var policy = Policy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) => { logger.LogWarning("{Message}", ex.ToString()); });
        var channel = await _objectPool.Get();
        if (channel == null)
        {
            throw new Exception("channel is null");
        }

        try
        {
            foreach (var message in messages)
            {
                try
                {
                    var props = new BasicProperties();
                    props.Headers ??= new Dictionary<string, object?>();
                    string exchangeNotify = $"{exchange}".ToLower();
                    var msg = EventBusMessageExtensions.EventBusMessageCreate(message, exchangeNotify,
                        message.EventId,
                        message.SerializeType == ESerializeType.Json
                            ? ESerializeType.Json
                            : ESerializeType.Protobuf, message.Publisher);
                    byte[] body;
                    if (message.SerializeType == ESerializeType.Json)
                    {
                        string json = JsonConverter.ToJson(msg);
                        body = Encoding.UTF8.GetBytes(json);
                    }
                    else
                    {
                        body = ProtoMode.ProtoBufSerialize(msg);
                    }

                    props.ContentType = msg.BodyType;
                    await policy.Execute(async () =>
                    {
                        try
                        {
                            await channel.BasicPublishAsync(exchangeNotify, "", false, props, body);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "{Message}", e.Message);
                            throw;
                        }
                    });
                }
                catch (Exception e)
                {
                    logger.LogError(e, "{Message}", e.Message);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
            throw;
        }
        finally
        {
            _objectPool.Return(channel);
        }
    }

    public async Task NotifyTrigger(string exchange, RabbitMqEventBusMessage message)
    {
        var policy = Policy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) => { logger.LogWarning("{Message}", ex.ToString()); });
        var channel = await _objectPool.Get();
        if (channel == null)
        {
            throw new Exception("channel is null");
        }

        try
        {
            var props = new BasicProperties();
            props.Headers ??= new Dictionary<string, object?>();
            string exchangeNotify = $"{exchange}".ToLower();
            byte[] body;
            if (message.SerializeType == ESerializeType.Json)
            {
                string json = JsonConverter.ToJson(message);
                body = Encoding.UTF8.GetBytes(json);
            }
            else
                body = ProtoMode.ProtoBufSerialize(message);

            props.ContentType = message.BodyType;
            await policy.Execute(async () =>
            {
                try
                {
                    await channel.BasicPublishAsync(exchangeNotify, $"", false, props, body);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "{Message}", e.Message);
                    throw;
                }
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
        }
        finally
        {
            _objectPool.Return(channel);
        }
    }

    public async Task RegisterExchangeAndQueue(string exchange, string[] routingKeys, string[] queues)
    {
        if (string.IsNullOrEmpty(exchange))
        {
            return;
        }

        logger.LogInformation("RegisterExchange:{Exchange}", exchange);
        if (!persistentConnection.IsConnected)
        {
            await persistentConnection.TryConnect();
        }

        var channel = await _objectPool.Get();
        if (channel == null)
        {
            throw new Exception("channel is null");
        }

        try
        {
            await channel.ExchangeDeclareAsync(exchange: exchange, type: ExchangeType.Topic, durable: true);
            int i = 0;
            foreach (var queueName in queues)
            {
                logger.LogInformation("RegisterQueue:{QueueName}", queueName);
                await channel.QueueDeclareAsync(queueName, true, false, false, null);
                await channel.QueueBindAsync(queue: queueName, exchange: exchange, routingKey: routingKeys[i]);
                i++;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
            throw;
        }
        finally
        {
            _objectPool.Return(channel);
        }
    }

    public async Task RegisterExchangeTrigger(string[] exchanges)
    {
        if (exchanges is not { Length: > 0 })
        {
            return;
        }

        if (!persistentConnection.IsConnected)
        {
            await persistentConnection.TryConnect();
        }

        var channel = await _objectPool.Get();
        if (channel == null)
        {
            throw new Exception("channel is null");
        }

        try
        {
            foreach (var exchange in exchanges)
            {
                if (exchange?.Length > 0)
                {
                    logger.LogInformation("RegisterExchangeTrigger:{Exchange}", exchange);
                    await channel.ExchangeDeclareAsync(exchange: exchange.ToLower(), type: ExchangeType.Fanout,
                        durable: true);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
            throw;
        }
        finally
        {
            _objectPool.Return(channel);
        }
    }

    public async Task<(IChannel? Channel, string? ConsumerTag )> SubscribeAsync(string queue,
        Func<byte[], Task> processFunc,
        int? rabbitMqPrefetchCount)
    {
        if (string.IsNullOrEmpty(queue))
        {
            return (null, null);
        }

        logger.LogInformation("CreateConsumerChannel: {QueueName}", queue);
        if (!persistentConnection.IsConnected)
        {
            await persistentConnection.TryConnect();
        }

        var channel = await _objectPool.Get();
        if (channel == null)
        {
            throw new Exception("channel is null");
        }

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                await processFunc(body);
                logger.LogInformation("ea.DeliveryTag: {EaDeliveryTag}", ea.DeliveryTag);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Rabbitmq Received error: {Message}", e.Message);
                //channel.BasicAck(ea.DeliveryTag, false);
            }
        };
        logger.LogInformation("RabbitMqPrefetchCount: {RabbitMqPrefetchCount}", rabbitMqPrefetchCount);
        if (rabbitMqPrefetchCount.GetValueOrDefault() < 0)
        {
            rabbitMqPrefetchCount = _rabbitMqPrefetchCount;
        }

        if (rabbitMqPrefetchCount.GetValueOrDefault() < 0)
        {
            rabbitMqPrefetchCount = 1;
        }

        await channel.BasicQosAsync(0, (ushort)rabbitMqPrefetchCount!.Value, true);
        string consumerTag = await channel.BasicConsumeAsync(queue, false, consumer);
        channel.CallbackExceptionAsync += (sender, ea) =>
        {
            logger.LogError("CallbackException:{ExceptionMessage}", ea.Exception.Message);
            channel.Dispose();
            return Task.CompletedTask;
        };
        return (channel, consumerTag);
    }

    public async Task RegisterExchange((string ExChange, string Type)[] exchanges)
    {
        if (exchanges is not { Length: > 0 })
            return;

        if (!persistentConnection.IsConnected)
            await persistentConnection.TryConnect();

        var channel = await _objectPool.Get();
        if (channel == null)
            throw new Exception("channel is null");

        try
        {
            foreach (var exchange in exchanges)
            {
                if (exchange.ExChange is not { Length: > 0 })
                    continue;
                
                logger.LogInformation("RegisterExchangeTrigger:{Exchange}", exchange);
                await channel.ExchangeDeclareAsync(exchange: exchange.ExChange.ToLower(), type: exchange.Type,
                    durable: true);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
            throw;
        }
        finally
        {
            _objectPool.Return(channel);
        }
    }

    public async Task RegisterExchange(string exChange, string type)
    {
        if (string.IsNullOrEmpty(exChange))
        {
            throw new Exception("exChange is null");
        }

        if (string.IsNullOrEmpty(type))
        {
            throw new Exception("type is null");
        }

        logger.LogInformation("RegisterExchange:{Exchange}", exChange);
        if (!persistentConnection.IsConnected)
        {
            await persistentConnection.TryConnect();
        }

        var channel = await _objectPool.Get();
        if (channel == null)
        {
            throw new Exception("channel is null");
        }

        try
        {
            await channel.ExchangeDeclareAsync(exchange: exChange.ToLower(), type: type, durable: true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
            throw;
        }
        finally
        {
            _objectPool.Return(channel);
        }
    }

    public async Task RegisterQueue(string exchange, (string Queue, string Routing)[] queues)
    {
        if (string.IsNullOrEmpty(exchange))
        {
            throw new Exception("exChange is null");
        }

        logger.LogInformation("RegisterExchange:{Exchange}", exchange);
        if (!persistentConnection.IsConnected)
        {
            await persistentConnection.TryConnect();
        }

        var channel = await _objectPool.Get();
        if (channel == null)
        {
            throw new Exception("channel is null");
        }

        try
        {
            await channel.ExchangeDeclareAsync(exchange: exchange, type: ExchangeType.Topic, durable: true);
            int i = 0;
            foreach (var item in queues)
            {
                logger.LogInformation("RegisterQueue:{QueueName} --- {Routing}", item.Queue, item.Routing);
                await channel.QueueDeclareAsync(item.Queue, true, false, false, null);
                await channel.QueueBindAsync(queue: item.Queue, exchange: exchange, routingKey: item.Routing);
                i++;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
            throw;
        }
        finally
        {
            _objectPool.Return(channel);
        }
    }

    public string GetHosts()
    {
        return persistentConnection.GetHosts();
    }

    public string Hosts => GetHosts();

    public async Task SubscribeExchangeAsync(string[] exchangesTrigger, Func<RabbitMqEventBusMessage, Task> processFunc)
    {
        if (exchangesTrigger is not { Length: > 0 })
        {
            return;
        }

        foreach (var exchange in exchangesTrigger)
        {
            if (exchange is not { Length: > 0 })
            {
                continue;
            }

            logger.LogInformation("CreateConsumerChannel: {Exchange}", exchange);
            await SubscribeNotifyAsync(exchange, processFunc);
        }
    }

    public async Task SubscribeAsync(string[] queues, Func<RabbitMqEventBusMessage, Task> processFunc)
    {
        foreach (var queueName in queues)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                continue;
            }

            logger.LogInformation("CreateConsumerChannel: {QueueName}", queueName);
            if (!persistentConnection.IsConnected)
            {
                await persistentConnection.TryConnect();
            }

            //var channel = persistentConnection.CreateModel();
            var channel = await _objectPool.Get();
            if (channel == null)
            {
                throw new Exception("channel is null");
            }

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                string? contentType = ea.BasicProperties.ContentType;
                var body = ea.Body.ToArray();
                RabbitMqEventBusMessage? msg = null;
                try
                {
                    msg = ProtoMode.ProtoBufDeserialize<RabbitMqEventBusMessage>(body);
                    if (msg != null)
                    {
                        await processFunc(msg);
                        logger.LogInformation("ea.DeliveryTag: {EventDeliveryTag}", ea.DeliveryTag);
                    }
                    else
                    {
                        logger.LogError("Rabbitmq Received msg is null: {ContentType}", contentType);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Rabbitmq Received error: {ContentType} {Message} {Msg}", contentType, e.Message,
                        JsonConverter.ToJson(msg));
                }
                finally
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
            };
            if (_rabbitMqPrefetchCount.GetValueOrDefault() < 0)
            {
                _rabbitMqPrefetchCount = 1;
            }

            logger.LogInformation("RabbitMqPrefetchCount: {RabbitMqPrefetchCount}", _rabbitMqPrefetchCount);
            await channel.BasicQosAsync(0, (ushort)_rabbitMqPrefetchCount!, true);
            //channel.BasicQos(0, 10, true);
            await channel.BasicConsumeAsync(queueName, false, consumer);
            channel.CallbackExceptionAsync += (sender, ea) =>
            {
                logger.LogError("CallbackException:{ExceptionMessage}", ea.Exception.Message);
                channel.Dispose();
                return Task.CompletedTask;
                //CreateConsumerChannel(actionString);
            };
        }
    }

    public async Task SubscribeAsync(string[] queues, Func<byte[], Task> processFunc)
    {
        foreach (var queueName in queues)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                continue;
            }

            logger.LogInformation("CreateConsumerChannel: {QueueName}", queueName);
            if (!persistentConnection.IsConnected)
            {
                await persistentConnection.TryConnect();
            }

            //var channel = persistentConnection.CreateModel();
            var channel = await _objectPool.Get();
            if (channel == null)
            {
                throw new Exception("channel is null");
            }

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                string? contentType = ea.BasicProperties.ContentType;
                try
                {
                    var body = ea.Body.ToArray();
                    await processFunc(body);
                    logger.LogInformation("ea.DeliveryTag: {EventDeliveryTag}", ea.DeliveryTag);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Rabbitmq Received error: {ContentType} {Message}", contentType, e.Message);
                    //channel.BasicAck(ea.DeliveryTag, false);
                }
                finally
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
            };
            if (_rabbitMqPrefetchCount.GetValueOrDefault() < 0)
            {
                _rabbitMqPrefetchCount = 1;
            }

            logger.LogInformation("RabbitMqPrefetchCount: {RabbitMqPrefetchCount}", _rabbitMqPrefetchCount);
            await channel.BasicQosAsync(0, (ushort)_rabbitMqPrefetchCount!, true);
            //channel.BasicQos(0, 10, true);
            await channel.BasicConsumeAsync(queueName, false, consumer);
            channel.CallbackExceptionAsync += (sender, ea) =>
            {
                logger.LogError("CallbackException:{ExceptionMessage}", ea.Exception.Message);
                channel.Dispose();
                return Task.CompletedTask;
                //CreateConsumerChannel(actionString);
            };
        }
    }

    private async Task SubscribeNotifyAsync(string exchange, Func<RabbitMqEventBusMessage, Task> processFunc)
    {
        if (exchange is not { Length: > 0 })
        {
            return;
        }

        if (!persistentConnection.IsConnected)
        {
            await persistentConnection.TryConnect();
        }

        //var channel = persistentConnection.CreateModel();
        var channel = await _objectPool.Get();
        if (channel == null)
        {
            throw new Exception("channel is null");
        }

        var queueName = (await channel.QueueDeclareAsync()).QueueName;
        await channel.QueueBindAsync(queue: queueName, exchange: exchange.ToLower(), routingKey: "");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            string? contentType = ea.BasicProperties.ContentType;
            RabbitMqEventBusMessage? msg = null;
            try
            {
                var body = ea.Body.ToArray();
                msg = ProtoMode.ProtoBufDeserialize<RabbitMqEventBusMessage>(body);
                if (msg != null)
                {
                    await processFunc(msg);
                    logger.LogInformation("ea.DeliveryTag: {EventDeliveryTag}", ea.DeliveryTag);
                }
                else
                {
                    logger.LogError("Rabbitmq Received msg is null: {ContentType}", contentType);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Rabbitmq Received error: {ContentType} {Message} {Msg}", contentType, e.Message,
                    JsonConverter.ToJson(msg));
                //channel.BasicAck(ea.DeliveryTag, false);
            }
        };
        // channel.BasicQos(0, 1, true);
        if (_rabbitMqPrefetchCount.GetValueOrDefault() < 0)
        {
            _rabbitMqPrefetchCount = 1;
        }

        await channel.BasicQosAsync(0, (ushort)_rabbitMqPrefetchCount!, true);
        await channel.BasicConsumeAsync(queueName, true, consumer);
        channel.CallbackExceptionAsync += (sender, ea) =>
        {
            logger.LogError("CallbackException:{ExceptionMessage}", ea.Exception.Message);
            channel.Dispose();
            return Task.CompletedTask;
            //CreateConsumerChannel(actionString);
        };
    }

    public async Task SubscribeNotifyAsync(string[] exchangesTrigger, Func<RabbitMqEventBusMessage, Task> processFunc)
    {
        if (exchangesTrigger is not { Length: > 0 })
        {
            return;
        }

        foreach (var exchange in exchangesTrigger)
        {
            if (exchange is not { Length: > 0 })
            {
                continue;
            }

            logger.LogInformation("CreateConsumerChannel: {Exchange}", exchange);
            await SubscribeNotifyAsync(exchange, processFunc);
        }
    }
}