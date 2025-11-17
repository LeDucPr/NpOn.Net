using CommonMode;
using CommonObject;
using CommonWebApplication;
using CommonWebApplication.Middlewares;
using Enums;
using SSO.Controllers;
using SSO.Middlewares;

namespace SSO;

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
        services.AddHostedService<HostingApp>();
        services.AddControllers(); 
        services.AddControllers()
            .AddApplicationPart(typeof(CommonProgram).Assembly); // CommonWebApplication

        return Task.CompletedTask;
    }

    // protected override void ConfigureBasePipeline(WebApplication app)
    // {
    //     base.ConfigureBasePipeline(app);
    // }
    

    protected override Task ConfigurePipeline(WebApplication app)
    {
        if (EApplicationConfiguration.IsUseMiddlewareLogger.GetAppSettingConfig().AsDefaultBool())
        {
            app.UseRequestResponseLogging();
        }

        // app.UseMiddleware<AuthenFilterHandlerMiddleware>();
        return Task.CompletedTask;
    }
}