using CommonMode;
using CommonObject;
using CommonWebApplication.Parameters;
using Enums;

namespace CommonWebApplication.Builders;

public static class BuilderExtensions
{
    public static async Task<WebApplication> AddAppConfig(this WebApplication services,
        Func<WebApplication, Task<WebApplication>>? configure)
    {
        if (configure != null)
            await WrapperProcessers.Processer(configure!, services);
        return services;
    }
    
    public static async Task<IServiceCollection> AddCollectionServices(this IServiceCollection services,
        Func<IServiceCollection, Task<IServiceCollection>>? configure)
    {
        if (configure != null)
            await WrapperProcessers.Processer(configure!, services);
        return services;
    }
}