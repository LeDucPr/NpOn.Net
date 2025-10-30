using ITZoneCallTestService;

namespace TZoneCallTestService.Services;

public class HostingApp(
    ILogger<HostingApp> logger,
    ICfCallTestService cfService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("LoyaltyManager AppHostedService is starting");
        await cfService.TestCallC();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("LoyaltyManager AppHostedService is stopping");
        return Task.CompletedTask;
    }
}