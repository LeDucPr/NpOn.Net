using DbFactory.Redis;
using IAccountService;

namespace AccountService.Services;

public class HostingApp(
    ILogger<HostingApp> logger,
    IRedisFactoryWrapper redisFactoryWrapper,
    IUserService userService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("AccountService AppHostedService is starting");

        // --- Ví dụ sử dụng Redis ---
        var key = "greeting:from:app";
        var value = $"Hello from AccountService at {DateTime.UtcNow:O}";

        logger.LogInformation("Setting Redis key '{Key}'", key);
        await redisFactoryWrapper.SetStringAsync(key, value, TimeSpan.FromMinutes(5));

        logger.LogInformation("Getting Redis key '{Key}'", key);
        var result = await redisFactoryWrapper.GetStringAsync(key);
        logger.LogInformation("Value from Redis: {Value}", result.Result);
        // var aaaa = (await userService.GetAccountInfos()).Data;
        // var aaaa = (await userService.GetAccountInfo()).Data;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("AccountService AppHostedService is stopping");
        return Task.CompletedTask;
    }
}