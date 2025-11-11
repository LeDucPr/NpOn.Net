using IFaCareTestService;

namespace FaCareTestService.Services;

public class HostingApp(
    ILogger<HostingApp> logger,
    IFaCareTService faCareTService
    ) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("LoyaltyManager AppHostedService is starting");
        // KHÔNG gọi TestCallSgnR ở đây vì nó không có context của client.
        var a = await faCareTService.TestCallSgnR();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("LoyaltyManager AppHostedService is stopping");
        return Task.CompletedTask;
    }
}