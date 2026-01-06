using MicroServices.General.Service.NpOn.IGeneralService;

namespace MicroServices.General.Service.NpOn.GeneralService.Services;

public class HostingApp(
    ILogger<HostingApp> logger
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("NpOn.GeneralService AppHostedService is starting");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("NpOn.GeneralService AppHostedService is stopping");
        return Task.CompletedTask;
    }
}