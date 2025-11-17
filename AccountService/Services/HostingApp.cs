using IAccountService;
using ITZoneService;

namespace AccountService.Services;

public class HostingApp(
    ILogger<HostingApp> logger,
    IUserService userService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("AccountService AppHostedService is starting");
        var aaaa = (await userService.GetAccountInfos()).Data;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("AccountService AppHostedService is stopping");
        return Task.CompletedTask;
    }
}