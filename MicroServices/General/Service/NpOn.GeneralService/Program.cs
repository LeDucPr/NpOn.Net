using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication;
using Common.Infrastructures.NpOn.DbFactory.Generics;
using MicroServices.General.Service.NpOn.GeneralService.Services;
using MicroServices.General.Service.NpOn.IGeneralService;

namespace MicroServices.General.Service.NpOn.GeneralService;

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
        services.AddSingleton<IDbFactoryWrapper>(_ =>
        {
            string connectionString =
                EApplicationConfiguration.ConnectionString.GetAppSettingConfig().AsDefaultString();
            int connectionNumber = EApplicationConfiguration.ConnectionNumber.GetAppSettingConfig().AsDefaultInt();
            IDbFactoryWrapper factoryWrapper =
                new DbFactoryWrapper(connectionString, EDb.Postgres, connectionNumber);
            return factoryWrapper;
        });

        if (EApplicationConfiguration.IsStartAsync.GetAppSettingConfig().AsDefaultBool())
        {
            services.AddHostedService<HostingApp>();
        }

        services.AddTransient<IFldMasterPgService, FldMasterPgService>();

        return Task.CompletedTask;
    }

    protected override void ConfigureBasePipeline(WebApplication app)
    {
        app.MapGet("/", () => "NpOn.GeneralService");
        base.ConfigureBasePipeline(app);
    }

    protected override Task ConfigurePipeline(WebApplication app)
    {
        app.MapGrpcService<FldMasterPgService>();
        return Task.CompletedTask;
    }
}