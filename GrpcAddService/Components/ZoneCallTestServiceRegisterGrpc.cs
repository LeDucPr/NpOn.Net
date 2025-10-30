using CommonMode;
using CommonObject;
using Enums;
using ITZoneCallTestService;

namespace GrpcAddService.Components;

public static partial class ServiceRegisterGrpc
{
    public static IServiceCollection ZoneCallTestServiceRegisterGrpc(this IServiceCollection services)
    {
        var tZoneServiceUrl =
            EApplicationConfiguration.TZoneServiceUrl.GetAppSettingConfig().AsDefaultString();
        if (string.IsNullOrWhiteSpace(tZoneServiceUrl))
            return services;
        services.RegisterGrpcClientLoadBalancing<ICfCallTestService>(tZoneServiceUrl);
        return services;
    }
}