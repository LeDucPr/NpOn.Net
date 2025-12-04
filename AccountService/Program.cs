using AccountService.Services;
using CommonMode;
using CommonObject;
using CommonWebApplication;
using DbFactory.Generics;
using DbFactory.Redis;
using Enums;
using IAccountService;

namespace AccountService;

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
                new DbFactoryWrapper(connectionString, EDb.Postgres, connectionNumber, true);
            // string stringQuery = "select * from Users where id = 'C000175'";
            // INpOnWrapperResult? resultOfQuery = factoryWrapper?.QueryAsync(stringQuery).GetAwaiter().GetResult();
            return factoryWrapper;
        });

        services.AddSingleton<IRedisFactoryWrapper, RedisFactoryWrapper>(factory =>
        {
            string connectionString = EApplicationConfiguration.RedisConnectString.GetAppSettingConfig().AsDefaultString();
            int connectionNumber = EApplicationConfiguration.RedisConnectionNumber.GetAppSettingConfig().AsDefaultInt();
            IRedisFactoryWrapper factoryWrapper =
                new RedisFactoryWrapper(connectionString, EDb.Redis, connectionNumber, true);
            return (RedisFactoryWrapper)factoryWrapper;
        });

        if (EApplicationConfiguration.IsStartAsync.GetAppSettingConfig().AsDefaultBool())
        {
            services.AddHostedService<HostingApp>();
        }

        // Add Service
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IAuthenticationService, AuthenticationService>();
        
        return Task.CompletedTask;
    }

    protected override void ConfigureBasePipeline(WebApplication app)
    {
        app.MapGet("/", () => "AccountService");
        base.ConfigureBasePipeline(app);
    }

    protected override Task ConfigurePipeline(WebApplication app)
    {
        // Add Map Grpc Service
        app.MapGrpcService<UserService>();
        app.MapGrpcService<AuthenticationService>();
        return Task.CompletedTask;
    }
}