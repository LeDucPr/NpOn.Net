using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommonWebApplication.Services;

/// <summary>
/// IHostedService chuyên dụng để khởi động và đăng ký các thành phần của RabbitMQ.
/// </summary>
public class RabbitMqBusStarter : IHostedService
{
    // private readonly ILogger<RabbitMqBusStarter> _logger;
    // private readonly IRabbitMqEventProcessor _eventProcessor;
    //
    // public RabbitMqBusStarter(ILogger<RabbitMqBusStarter> logger, IRabbitMqEventProcessor eventProcessor)
    // {
    //     _logger = logger;
    //     _eventProcessor = eventProcessor;
    // }
    //
    // public Task StartAsync(CancellationToken cancellationToken)
    // {
    //     _logger.LogInformation("RabbitMQ Bus Starter is starting.");
    //     _eventProcessor.Register();
    //     return _eventProcessor.Start();
    // }
    //
    // public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}