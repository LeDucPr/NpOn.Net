using System.Net;
using System.Security.Cryptography;
using CommonMode;
using CommonObject;
using CommonWebApplication;
using CommonWebApplication.Middlewares;
using Enums;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.IdentityModel.Logging;
using Microsoft.Net.Http.Headers;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

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
        
        return Task.CompletedTask;
    }

    protected override void ConfigureBasePipeline(WebApplication app)
    {
        string appName = EApplicationConfiguration.AppName.GetAppSettingConfig().AsDefaultString();
        app.MapGet("/", () => appName);
        base.ConfigureBasePipeline(app);
    }

    protected override Task ConfigurePipeline(WebApplication app)
    {
        if (EApplicationConfiguration.IsUseMiddlewareLogger.GetAppSettingConfig().AsDefaultBool())
        {
            app.UseRequestResponseLogging();
        }
        return Task.CompletedTask;
    }
}