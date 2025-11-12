using System.Collections.Concurrent;
using CommonObject;
using Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMqBroker;

namespace TYT.EventBus;

public class RabbitMqEventProcessor : IRabbitMqEventProcessor
{
    private readonly ConcurrentDictionary<Type, HashSet<IRabbitMqMessageHandler>> _handlers = [];
    private readonly ConcurrentDictionary<string, HashSet<string>> _eventHandlerFilterQueues = [];
    private readonly ILogger _logger;
    private readonly IRabbitMqEventStorageRepository? _eventStorageRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqConnectionPool _rabbitMqConnection;

    private readonly string _exchange;
    private readonly string _rabbitMqExChangeNotify;
    private readonly string _eventHandlerFilterQueuesConfig;
    private readonly string[] _routingKeys;
    private readonly string[] _topics;
    private readonly string _rabbitMqExChangeNotifyListen;
    private readonly string[] _exChangesTrigger;
    private readonly string[] _workerGroup;

    public RabbitMqEventProcessor(
        ILogger<RabbitMqEventProcessor> logger,
        IRabbitMqEventStorageRepository? eventStorageRepository,
        IServiceProvider serviceProvider,
        RabbitMqConnectionPool rabbitMqConnection,
        string exchange,
        string rabbitMqExChangeNotify,
        string eventHandlerFilterQueuesConfig,
        string[] routingKeys,
        string[] topics,
        string rabbitMqExChangeNotifyListen,
        string[] exChangesTrigger,
        string[] workerGroup
    )
    {
        _logger = logger;
        _eventStorageRepository = eventStorageRepository;
        _serviceProvider = serviceProvider;
        _rabbitMqConnection = rabbitMqConnection;

        _exchange = exchange;
        _rabbitMqExChangeNotify = rabbitMqExChangeNotify;
        _eventHandlerFilterQueuesConfig = eventHandlerFilterQueuesConfig;
        _routingKeys = routingKeys;
        _topics = topics;
        _rabbitMqExChangeNotifyListen = rabbitMqExChangeNotifyListen;
        _exChangesTrigger = exChangesTrigger;
        _workerGroup = workerGroup;
    }

    // private readonly string _exChange = ConfigSettingEnum.RabbitMqExChange.GetConfig();
    // private readonly string _rabbitMqExChangeNotify = ConfigSettingEnum.RabbitMqExChangeNotify.GetConfig();
    // private readonly string _eventHandlerFilterQueuesConfig = ConfigSettingEnum.EventHandlerFilterQueues.GetConfig().ToLower();
    // private readonly string[] _routingKeys = ConfigSettingEnum.RabbitMqRouting.GetConfig().Split(',', StringSplitOptions.RemoveEmptyEntries);
    // readonly string[] _topics = ConfigSettingEnum.RabbitMqQueues.GetConfig().Split(',', StringSplitOptions.RemoveEmptyEntries);
    // private readonly string _rabbitMqExChangeNotifyListen = ConfigSettingEnum.RabbitMqExChangeNotifyListen.GetConfig();
    // readonly string[] _exChangesTrigger = ConfigSettingEnum.RabbitMqExChangeTriggerListen.GetConfig().Split(',', StringSplitOptions.RemoveEmptyEntries);\
    //private readonly string[] _workerGroup = ConfigSettingEnum.WorkerGroup.GetConfig().ToLower().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);


    public void Register()
    {
        if (_eventHandlerFilterQueuesConfig.Length > 0)
        {
            var configs = _eventHandlerFilterQueuesConfig.Split(",",
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (configs.Length > 0)
            {
                foreach (var config in configs)
                {
                    var configsByQueue = config.Split("|",
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (configsByQueue.Length > 1)
                    {
                        if (!_eventHandlerFilterQueues.ContainsKey(configsByQueue[0]))
                        {
                            _eventHandlerFilterQueues.TryAdd(configsByQueue[0], []);
                        }

                        for (int i = 1; i < configsByQueue.Length; i++)
                        {
                            _eventHandlerFilterQueues[configsByQueue[0]].Add(configsByQueue[i]);
                        }
                    }
                }
            }
        }

        var genericHandler = typeof(IRabbitMqMessageHandler<>);
        var services = _serviceProvider.GetServices<IRabbitMqMessageHandler>();
        foreach (IRabbitMqMessageHandler service in services)
        {
            if (_workerGroup.Length > 0)
            {
                if (!_workerGroup.Contains(service.WorkerGroup.ToLower()))
                    continue;
            }

            var supportedCommandTypes = service.GetType()
                .GetInterfaces()
                .Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == genericHandler)
                .Select(iface => iface.GetGenericArguments()[0])
                .ToArray();
            if (supportedCommandTypes.Length > 0)
            {
                foreach (var commandType in supportedCommandTypes)
                {
                    if (!_handlers.ContainsKey(commandType))
                    {
                        bool result = _handlers.TryAdd(commandType, new HashSet<IRabbitMqMessageHandler>());
                    }

                    bool result1 = _handlers[commandType].Add(service);
                }
            }
            else
            {
                var baseTypes = service.GetType().GetInterfaces();
                foreach (var baseType in baseTypes)
                {
                    if (baseType == typeof(IRabbitMqMessageHandler))
                    {
                        if (!_handlers.ContainsKey(typeof(string)))
                        {
                            bool result = _handlers.TryAdd(typeof(string), new HashSet<IRabbitMqMessageHandler>());
                        }

                        bool result1 = _handlers[typeof(string)].Add(service);
                    }
                }
            }
        }
    }

    public async Task Start()
    {
        await _rabbitMqConnection.RegisterExchange(_exchange, ExchangeType.Topic);
        await _rabbitMqConnection.RegisterExchange(_rabbitMqExChangeNotify, ExchangeType.Fanout);
        (string Queue, string Routing)[] queues = new (string Queue, string Routing)[_topics.Length];
        int i = 0;
        foreach (var topic in _topics)
        {
            queues[i] = (topic, _routingKeys[i]);
            i++;
        }

        await _rabbitMqConnection.RegisterQueue(_exchange, queues);
        if (_exChangesTrigger.Length > 0)
        {
            (string ExChange, string Type)[] exchanges = new (string ExChange, string Type)[_exChangesTrigger.Length];
            i = 0;
            foreach (var exChangeTrigger in _exChangesTrigger)
            {
                exchanges[i] = (exChangeTrigger, ExchangeType.Fanout);
                i++;
            }

            await _rabbitMqConnection.RegisterExchange(exchanges);
        }

        await _rabbitMqConnection.SubscribeQueueAsync(_topics, ProcessMessage);
        await _rabbitMqConnection.SubscribeExchangeAsync(_exChangesTrigger, ProcessMessage);
        if (_rabbitMqExChangeNotifyListen.Length > 0)
        {
            await _rabbitMqConnection.SubscribeExchangeAsync([_rabbitMqExChangeNotifyListen], ProcessMessage);
        }
    }

    private async Task ProcessMessage(RabbitMqEventBusMessage message)
    {
        var messageProcess = message;
        var processDate = DateTime.UtcNow;
        messageProcess.ProcessDate = processDate;
        messageProcess.SendTime = messageProcess.ProcessDate?.Subtract(messageProcess.CreatedDate)
            .TotalMilliseconds.AsDefaultLong();
        try
        {
            _logger.LogInformation("ProcessMessage: {MessageProcessBodyType}", messageProcess.BodyType);
            ERabbitMqEventStatus status = ERabbitMqEventStatus.New;
            string error = string.Empty;
            var result = new Dictionary<string, string>();
            try
            {
                result = Handle(messageProcess);
                status = ERabbitMqEventStatus.Success;
            }
            catch (Exception e)
            {
                status = ERabbitMqEventStatus.Fail;
                error = $"Exception:{e.Message}:{e.StackTrace}";
                _logger.LogError(e, "{Error}", error);
            }
            finally
            {
                if (!messageProcess.BodyType?.StartsWith("TYT.BaseApplication.Models.LogActionModel") == true)
                {
                    try
                    {
                        messageProcess.FinishDate = DateTime.UtcNow;
                        messageProcess.ExecuteTime = messageProcess.FinishDate?.Subtract(processDate)
                            .TotalMilliseconds.AsDefaultLong();
                        messageProcess.Status = status;
                        messageProcess.Error = $"{error} - {result.Values.ToArray().AsArrayJoin()}";
                        messageProcess.Consumer = $"{Environment.MachineName} - {result.Keys.ToArray().AsArrayJoin()}";
                        // await _eventStorageRepository?.Add(messageProcess, status, error);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "ProcessMessage finally {Message}", e.Message);
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "ProcessMessage Exception {Message}", e.Message);
            throw;
        }
    }

    public Dictionary<string, string> Handle(RabbitMqEventBusMessage message)
    {
        var result = new Dictionary<string, string>();
        var commandType = Type.GetType(message.BodyType.AsEmptyString());
        if (commandType != null && _handlers.TryGetValue(commandType, out var handlers))
        {
            using var scope = _serviceProvider.CreateScope();
            if (handlers.Count <= 0) return result;
            List<Task> tasks = [];
            foreach (var handler in handlers)
            {
                var handlerType = handler.GetType();
                var consumer = handlerType.FullName;
                string handlerName = handlerType.Name.ToLower();
                if (_eventHandlerFilterQueues.TryGetValue(handlerName, out var queues) && queues.Count > 0)
                {
                    if (!queues.Contains(message.TopicName.ToLower()))
                    {
                        continue;
                    }
                }

                var error = string.Empty;
                try
                {
                    var service = scope.ServiceProvider.GetRequiredService(handlerType);
                    if (service == null)
                    {
                        throw new Exception("Handler not register");
                    }

                    _logger.LogInformation("Handle BodyType:{MessageBodyType}", message.BodyType);
                    var task = ((dynamic)service).Handle(
                        (dynamic)message.EventBusMessageToObj()!, message.TopicName);
                    tasks.Add(task);
                }
                catch (Exception e)
                {
                    error = $"Exception:{e.Message}:{e.StackTrace}";
                    _logger.LogError(e, "{Message}", e.Message);
                }

                result.Add(consumer.AsDefaultString(), error);
            }

            if (tasks.Count > 0)
            {
                Task.WhenAll(tasks.ToArray());
            }
        }
        else
        {
            var logMessage = $"No Handler for {message.BodyType}";
            _logger.LogInformation("{LogMessage}", logMessage);
            result.Add("No Handler", logMessage);
        }

        return result;
    }
}