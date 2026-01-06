using Common.Extensions.NpOn.CommonWebApplication.Services;

namespace Controllers.NpOn.SSO.Controllers;

public class HostingApp(
    PermissionService permissionService,
    ILogger<HostingApp> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            bool success = false;
            while (!success && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    success = await permissionService.AutoSyncPermissionController();
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Connect Error (Timeout): {ex.Message}");
                }
                if (!success) await Task.Delay(1000, cancellationToken); // delay has token to cancel
            }
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}