using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication;
using Common.Extensions.NpOn.CommonWebApplication.Middlewares;
using Controllers.NpOn.SSO.Controllers;

namespace Controllers.NpOn.SSO;

public sealed class Program : CommonProgram
{
    private Program(string[] args) : base(args)
    {
    }

    public static async Task Main(string[] args)
    {
        Program program = new Program(args);
        await program.RunAsync();
    }

    protected override Task ConfigureServices(IServiceCollection services)
    {
        // services.AddTransient<AuthenFilterHandlerMiddleware>();
        if (EApplicationConfiguration.IsStartAsync.GetAppSettingConfig().AsDefaultBool())
        {
            services.AddHostedService<HostingApp>();
        }

        services.AddControllers();
        services.AddControllers()
            .AddApplicationPart(typeof(CommonProgram).Assembly); // NpOn.CommonWebApplication

        return Task.CompletedTask;
    }

    protected override Task ConfigurePipeline(WebApplication app)
    {
        if (EApplicationConfiguration.IsUseMiddlewareLogger.GetAppSettingConfig().AsDefaultBool())
        {
            app.UseRequestResponseLogging();
        }

        app.UseTokenValidation();
        app.UsePermissionValidation();

        // app.UseMiddleware<AuthenFilterHandlerMiddleware>();
        return Task.CompletedTask;
    }
}