namespace GeneralService.Services;

public class HostingApp(
    ILogger<HostingApp> logger
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("AccountService AppHostedService is starting");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("AccountService AppHostedService is stopping");
        return Task.CompletedTask;
    }
}