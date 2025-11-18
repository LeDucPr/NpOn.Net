using CommonMode;
using CommonObject;
using Enums;
using IAccountService;

namespace GrpcAddService.Components;

public static partial class ServiceRegisterGrpc
{
    public static IServiceCollection AccountServiceRegisterGrpc(this IServiceCollection services)
    {
        var accountServiceUrl =
            EApplicationConfiguration.AccountServiceUrl.GetAppSettingConfig().AsDefaultString();
        if (string.IsNullOrWhiteSpace(accountServiceUrl)) 
            return services;
        services.RegisterGrpcClientLoadBalancing<IUserService>(accountServiceUrl);
        services.RegisterGrpcClientLoadBalancing<IAuthenticationService>(accountServiceUrl);
        return services;
    }
}