using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using MicroServices.Account.Service.NpOn.IAccountService;

namespace Definitions.NpOn.GrpcAddService.Components;

public static partial class ServiceRegisterGrpc
{
    public static IServiceCollection AccountServiceRegisterGrpc(this IServiceCollection services)
    {
        var accountServiceUrl =
            EApplicationConfiguration.AccountServiceUrl.GetAppSettingConfig().AsDefaultString();
        if (string.IsNullOrWhiteSpace(accountServiceUrl))
            return services;
        services.RegisterGrpcClientLoadBalancing<IAccountInfoService>(accountServiceUrl);
        services.RegisterGrpcClientLoadBalancing<IAuthenticationService>(accountServiceUrl);
        services.RegisterGrpcClientLoadBalancing<IAccountPermissionService>(accountServiceUrl);
        services.RegisterGrpcClientLoadBalancing<IAccountGroupService>(accountServiceUrl);
        services.RegisterGrpcClientLoadBalancing<IAccountMenuService>(accountServiceUrl);
        services.RegisterGrpcClientLoadBalancing<IAccountMenuPermissionService>(accountServiceUrl);
        
        return services;
    }
}