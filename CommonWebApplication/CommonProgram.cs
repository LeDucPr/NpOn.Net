using CommonMode;
using CommonObject;
using CommonWebApplication.Builders;
using CommonWebApplication.Parameters;
using Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommonWebApplication;

public abstract class CommonProgram
{
    protected readonly string[] Args;
    protected IConfiguration AppConfig;

    protected CommonProgram(string[] args)
    {
        Args = args;
    }

    public async Task RunAsync()
    {
        var builder = CreateDefaultBuilder(Args);
        AppConfig = builder.Configuration;
        await builder.Services.AddCollectionServices(async (services) =>
        {
            ConfigureBaseServices(services);
            await ConfigureServices(services);
            return services;
        });

        var app = builder.Build();

        await app.AddAppConfig(async (appConfig) =>
        {
            ConfigureBasePipeline(appConfig);
            await ConfigurePipeline(appConfig);
            return appConfig;
        });
        await app.RunAsync(); // run
    }

    /// <summary>
    /// Configures services that are common to all applications.
    /// </summary>
    protected virtual void ConfigureBaseServices(IServiceCollection services)
    {
        services.AddGrpc();
    }

    /// <summary>
    /// Configures services specific to the derived application.
    /// </summary>
    protected abstract Task ConfigureServices(IServiceCollection services);

    /// <summary>
    /// Configures the common parts of the HTTP request pipeline.
    /// </summary>
    protected virtual void ConfigureBasePipeline(WebApplication app)
    {
        app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        if (this.GetType() == typeof(CommonProgram))
            throw new Exception($"{nameof(CommonProgram.ConfigureBasePipeline)} need override.");
    }

    /// <summary>
    /// Configures the HTTP request pipeline specific to the derived application (e.g., mapping gRPC services).
    /// </summary>
    protected abstract Task ConfigurePipeline(WebApplication app);
    
    private WebApplicationBuilder CreateDefaultBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // host-domain-start
        string hostDomain = builder.Configuration.TryGetConfig(EApplicationConfiguration.HostDomain).AsDefaultString();
        var hostPort = builder.Configuration.TryGetConfig(EApplicationConfiguration.HostPort).AsDefaultInt();
        if (hostPort > 0)
            hostDomain = $"{hostDomain}:{hostPort}";
        if (string.IsNullOrWhiteSpace(hostDomain))
            throw new Exception(EWebApplicationError.HostDomain.GetDisplayName());
        builder.WebHost.UseUrls($"{hostDomain}:{hostPort}");
        return builder;
    }
}