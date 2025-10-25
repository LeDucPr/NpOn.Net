using BaseWebApplication.Parameters;
using BaseWebApplication.Services;
using CommonMode;
using CommonObject;
using Enums;

namespace BaseWebApplication.Builders;

public static class BuilderExtensions
{
    public static async Task<IServiceCollection> AddCollectionServices(this IServiceCollection services, Func<IServiceCollection, Task<IServiceCollection>>? configure)
    {
        if (configure != null)
            await WrapperProcessers.Processer(configure!, services);
        return services;
    }

    public static async Task<WebApplicationBuilder> CreateDefaultBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // host-domain-start
        string hostDomain = builder.Configuration.TryGetConfig(EConfiguration.HostDomain).AsDefaultString();
        var hostPort = builder.Configuration.TryGetConfig(EConfiguration.HostPort).AsDefaultInt();
        if (hostPort > 0)
            hostDomain = $"{hostDomain}:{hostPort}";
        if (string.IsNullOrWhiteSpace(hostDomain))
            throw new Exception(EWebApplicationError.HostDomain.GetDisplayName());
        builder.WebHost.UseUrls($"{hostDomain}:{hostPort}");
        
        return builder;
    }
    
    // public static async Task<IApplicationBuilder> Build(this IApplicationBuilder builder)
    // {
    //     
    // }
}