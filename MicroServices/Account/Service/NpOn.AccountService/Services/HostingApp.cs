namespace MicroServices.Account.Service.NpOn.AccountService.Services;

public class HostingApp(
    ILogger<HostingApp> logger
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("NpOn.AccountService AppHostedService is starting");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("NpOn.AccountService AppHostedService is stopping");
        return Task.CompletedTask;
    }
}