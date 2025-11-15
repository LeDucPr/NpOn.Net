using ITZoneService;

namespace AccountService.Services;

public class HostingApp(
    ILogger<HostingApp> logger,
    ICfService cfService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("LoyaltyManager AppHostedService is starting");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("LoyaltyManager AppHostedService is stopping");
        return Task.CompletedTask;
    }
}