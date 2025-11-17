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
        services.AddTransient<SSO.Middlewares.AuthenFilterHandlerMiddleware>();
        services.AddHostedService<HostingApp>();
        // services.AddControllers(); 
        services.AddControllers()
            .AddApplicationPart(typeof(CommonProgram).Assembly); // Thêm dòng này để đăng ký controller từ project CommonWebApplication

        return Task.CompletedTask;
    }

    protected override void ConfigureBasePipeline(WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        string appName = EApplicationConfiguration.AppName.GetAppSettingConfig().AsDefaultString();
        app.MapGet("/", () => appName);
        // base.ConfigureBasePipeline(app);
    }

    protected override Task ConfigurePipeline(WebApplication app)
    {
        if (EApplicationConfiguration.IsUseMiddlewareLogger.GetAppSettingConfig().AsDefaultBool())
        {
            app.UseRequestResponseLogging();
        }

        app.UseAuthenFilterHandlerMiddleware();
        return Task.CompletedTask;
    }
}