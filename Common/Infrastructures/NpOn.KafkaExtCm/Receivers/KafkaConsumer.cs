using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.KafkaExtCm.Configs;
using Common.Infrastructures.NpOn.KafkaExtCm.Events;
using Common.Infrastructures.NpOn.KafkaExtCm.Generics;
using Common.Infrastructures.NpOn.KafkaExtCm.Topics;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructures.NpOn.KafkaExtCm.Receivers;

public abstract class KafkaConsumer<T> : KafkaComponent<T>, IKafkaConsumer<T>, IDisposable
    where T : CommonMessageContent
{
    private readonly ILogger<KafkaConsumer<T>>? _logger;
    private readonly IKafkaTopic _kafkaTopic;
    private IConsumer<string, byte[]>? _consumer;
    private CancellationTokenSource? _cts;
    private readonly bool _autoAck;
    private ERabbitMqResponseType _responseType = ERabbitMqResponseType.BasicAck;

    protected Func<T, Task>? Handler;

    protected KafkaConsumer(IKafkaTopic kafkaTopic, bool autoAck = true,
        ILogger<KafkaConsumer<T>>? logger = null)
    {
        _kafkaTopic = kafkaTopic;
        _logger = logger;
        _autoAck = autoAck;

        AddHandler(); // same as RabbitMQ: set Handler before assigned consumer
        UseDefault().GetAwaiter().GetResult();
    }

    public async Task UseDefault(bool isDecompress = false)
    {
        if (!IsEnableType) return;

        var config = new ConsumerConfig(_kafkaTopic.GetKafkaConfig());

        foreach (var item in KafkaDefaultConfig.ConsumerConfigs.Values)
        {
            if (!config.Any(x => x.Key == item.Key))
            {
                config.Set(item.Key, item.DefaultValue);
            }
        }

        config.AutoOffsetReset = AutoOffsetReset.Earliest;

        if (string.IsNullOrEmpty(config.GroupId))
            config.GroupId = $"{TopicName}.{PartitionName}.{IndexerMode.CreateGuid()}";

        config.EnableAutoCommit = false; // If the application crashes while running, it's problematic.

        _consumer = new ConsumerBuilder<string, byte[]>(config).Build();
        _consumer.Subscribe(TopicName);
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => ConsumeLoop(isDecompress, _cts.Token));
        await Task.CompletedTask;
    }

    private async Task ConsumeLoop(bool isDecompress, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var cr = _consumer!.Consume(token);
                if (cr == null) continue;

                var fullEvent = ProtoBufMode.ProtoBufDeserialize<KafkaEvent<T>>(cr.Message.Value, isDecompress);
                if (fullEvent?.MessageContent == null || Handler == null) continue;

                await HandleMessage(cr, fullEvent.MessageContent);
            }
            catch (OperationCanceledException)
            {
                break; // Stop loop gracefully
            }
            catch (ConsumeException e)
            {
                _logger?.LogError($"Consume error: {e.Error.Reason}");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error in ConsumeLoop: {ex.Message}");
            }
        }
        
        _consumer?.Close();
    }

    private async Task HandleMessage(ConsumeResult<string, byte[]> cr, T message)
    {
        try
        {
            if (_autoAck)
            {
                await ProcessWithAck(cr, message);
                return;
            }

            // if not autoAck, run in background and commit based on responseType
            _ = Task.Run(() => Handler!(message));
            CommitByResponseType(cr);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Handler error: {ex.Message}"); // Do not commit to allow the message to be reprocessed
        }
    }

    private async Task ProcessWithAck(ConsumeResult<string, byte[]> cr, T message)
    {
        await Handler!(message);
        _consumer!.Commit(cr);
    }

    private void CommitByResponseType(ConsumeResult<string, byte[]> cr)
    {
        switch (_responseType)
        {
            case ERabbitMqResponseType.BasicAck:
                _consumer!.Commit(cr);
                break;
            case ERabbitMqResponseType.BasicNack:
                // Kafka does not have nack, can Seek offset if reprocessing is desired
                break;
            case ERabbitMqResponseType.BasicReject:
                // Kafka does not have reject, can skip commit to reprocess
                break;
            case ERabbitMqResponseType.Default:
            default:
                // no commit
                break;
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _consumer?.Dispose();
        _cts?.Dispose();
    }

    /// <summary>
    /// Similar to RabbitMQ: child class overrides to register message handler.
    /// </summary>
    public abstract void AddHandler();
}