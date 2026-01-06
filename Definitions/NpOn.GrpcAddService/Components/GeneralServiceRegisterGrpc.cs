using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using MicroServices.General.Service.NpOn.IGeneralService;

namespace Definitions.NpOn.GrpcAddService.Components;

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