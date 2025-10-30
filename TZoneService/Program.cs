using CommonMode;
using CommonObject;
using CommonWebApplication;
using CommonWebApplication.Parameters;
using DbFactory;
using Enums;
using ITZoneService;
using ProtoBuf.Grpc.Server;
using TZoneService.Services;

namespace TZoneService;

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
        services.AddTransient<ICfService, CfService>();
        
        return Task.CompletedTask;
    }

    protected override void ConfigureBasePipeline(WebApplication app)
    {
        app.MapGet("/", () => "TZoneService");
        base.ConfigureBasePipeline(app);
    }

    protected override Task ConfigurePipeline(WebApplication app)
    {
        // Add Map Grpc Service
        app.MapGrpcService<CfService>();
        return Task.CompletedTask;
    }
}