using Enums;
using CommonMode;
using CommonObject;
using Enums;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Grpc.Net.ClientFactory;
using ITZoneService;
using ProtoBuf.Grpc.ClientFactory;

namespace GrpcAddService.Components;

public static partial class ServiceRegisterGrpc
{
    public static IServiceCollection ZoneServiceRegisterGrpc(this IServiceCollection services)
    {
        var tZoneServiceUrl =
            EApplicationConfiguration.TZoneServiceUrl.GetAppSettingConfig().AsDefaultString();
        if (string.IsNullOrWhiteSpace(tZoneServiceUrl)) 
            return services;
        services.RegisterGrpcClientLoadBalancing<ICfService>(tZoneServiceUrl);
        return services;
    }
}