using CommonDb.Connections;
using CommonDb.DbResults;
using CommonObject;
using CommonWebApplication;
using CommonWebApplication.Parameters;
using DbFactory;
using Enums;

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
        // services.AddSingleton<IDbFactoryWrapper>(factory =>
        // {
        //     string connectionString =
        //         AppConfig.TryGetConfig(EApplicationConfiguration.ConnectionString).AsDefaultString();
        //     int connectionNumber = AppConfig.TryGetConfig(EApplicationConfiguration.ConnectionNumber).AsDefaultInt();
        //     IDbFactoryWrapper factoryWrapper =
        //         new DbFactoryWrapper(connectionString, EDb.Mssql, connectionNumber, true);
        //     
        //     string stringQuery = "select * from Users where id = 'C000175'";
        //     INpOnWrapperResult? resultOfQuery = factoryWrapper?.QueryAsync(stringQuery).GetAwaiter().GetResult();
        //
        //     return factoryWrapper;
        // });
        
        string connectionString =
            AppConfig.TryGetConfig(EApplicationConfiguration.ConnectionString).AsDefaultString();
        int connectionNumber = AppConfig.TryGetConfig(EApplicationConfiguration.ConnectionNumber).AsDefaultInt();
        IDbFactoryWrapper factoryWrapper =
            new DbFactoryWrapper(connectionString, EDb.Mssql, connectionNumber, true);
            
        string stringQuery = "select * from Users where id = 'C000175'";
        INpOnWrapperResult? resultOfQuery = factoryWrapper?.QueryAsync(stringQuery).GetAwaiter().GetResult();
        
        
        
        
        string connectionString1 =
            AppConfig.TryGetConfig(EApplicationConfiguration.ConnectionString).AsDefaultString();
        int connectionNumber1 = AppConfig.TryGetConfig(EApplicationConfiguration.ConnectionNumber).AsDefaultInt();
        IDbFactoryWrapper factoryWrapper1 =
            new DbFactoryWrapper(connectionString, EDb.Mssql, connectionNumber, true);
            
        string stringQuery1 = "select * from Users where id = 'C000175'";
        INpOnWrapperResult? resultOfQuery1 = factoryWrapper?.QueryAsync(stringQuery).GetAwaiter().GetResult();
        
        


        return Task.CompletedTask;
    }

    protected override void ConfigureBasePipeline(WebApplication app)
    {
        app.MapGet("/", () => "TZoneService");
        base.ConfigureBasePipeline(app);
    }

    protected override Task ConfigurePipeline(WebApplication app)
    {
        // app.MapGrpcService<GreeterService>();
        return Task.CompletedTask;
    }
}