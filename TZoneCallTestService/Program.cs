using CommonMode;
using CommonObject;
using CommonWebApplication;
using DbFactory;
using Enums;
using ITZoneCallTestService;
using TZoneCallTestService.Services;

namespace TZoneCallTestService;

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
        services.AddSingleton<IDbFactoryWrapper>(factory =>
        {
            string connectionString = EApplicationConfiguration.ConnectionString.GetAppSettingConfig().AsDefaultString();
            int connectionNumber = EApplicationConfiguration.ConnectionNumber.GetAppSettingConfig().AsDefaultInt();
            IDbFactoryWrapper factoryWrapper =
                new DbFactoryWrapper(connectionString, EDb.Mssql, connectionNumber, true);
            // string stringQuery = "select * from Users where id = 'C000175'";
            // INpOnWrapperResult? resultOfQuery = factoryWrapper?.QueryAsync(stringQuery).GetAwaiter().GetResult();
            return factoryWrapper;
        });

        if (EApplicationConfiguration.IsStartAsync.GetAppSettingConfig().AsDefaultBool())
        {
            services.AddHostedService<HostingApp>();
        }

        // Add Service
        services.AddTransient<ICfCallTestService, CfCallTestService>();
        
        return Task.CompletedTask;
    }

    protected override void ConfigureBasePipeline(WebApplication app)
    {
        app.MapGet("/", () => "TZoneCallTestService");
        base.ConfigureBasePipeline(app);
    }

    protected override Task ConfigurePipeline(WebApplication app)
    {
        // Add Map Grpc Service
        app.MapGrpcService<CfCallTestService>();
        return Task.CompletedTask;
    }
}