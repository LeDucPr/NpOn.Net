using FaCareTestServiceObject.ServiceObjects.RabbitMqEvents;
using IFaCareTestService;
using RabbitMqBroker;

namespace FaCareTestService.Handlers.RabbitMqHandlers;

public class TestQueueRabbitMqHandler(
    ILogger<TestQueueRabbitMqHandler> logger,
    ITestQueueService testQueueService
    ) : IRabbitMqMessageHandler<RabbitMqTestEvent>
{
    public async Task Handle(RabbitMqTestEvent message, string topic)
    {
        await testQueueService.ProcessEventRbMqT2(message);
    }

    public string WorkerGroup { get; } = "tzone";
}