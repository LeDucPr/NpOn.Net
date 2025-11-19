using CommonMode;
using CommonObject;
using Enums;
using IAccountService;
using IGeneralService;

namespace GrpcAddService.Components;

public static partial class ServiceRegisterGrpc
{
    public static IServiceCollection GeneralServiceRegisterGrpc(this IServiceCollection services)
    {
        var generalServiceUrl =
            EApplicationConfiguration.GeneralServiceUrl.GetAppSettingConfig().AsDefaultString();
        if (string.IsNullOrWhiteSpace(generalServiceUrl)) 
            return services;
        services.RegisterGrpcClientLoadBalancing<IFldMasterPgService>(generalServiceUrl);
        return services;
    }
}