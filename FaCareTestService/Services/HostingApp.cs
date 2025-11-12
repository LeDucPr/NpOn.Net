using IFaCareTestService;

namespace FaCareTestService.Services;

public class HostingApp(
    ILogger<HostingApp> logger,
    IFaCareTService faCareTService, 
    ITestQueueService testQueueService
    ) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("T2C AppHostedService is starting");
        // var a = await faCareTService.TestCallSgnR();
        await testQueueService.TestQueue2C();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("T2C AppHostedService is stopping");
        return Task.CompletedTask;
    }
}