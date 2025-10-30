using ITZoneService;

namespace TZoneService.Services;

public class HostingApp(
    ILogger<HostingApp> logger,
    ICfService cfService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("LoyaltyManager AppHostedService is starting");
        var a = await cfService.CCC();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("LoyaltyManager AppHostedService is stopping");
        return Task.CompletedTask;
    }
}