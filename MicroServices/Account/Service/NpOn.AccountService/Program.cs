using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Common.Infrastructures.NpOn.BaseRepository.Postgres;
using Common.Infrastructures.NpOn.DbFactory.Generics;
using Common.Infrastructures.NpOn.DbFactory.Redis;
using MicroServices.Account.Service.NpOn.AccountService.KafkaConsumers;
using MicroServices.Account.Service.NpOn.AccountService.RabbitMqConsumers;
using MicroServices.Account.Service.NpOn.AccountService.Services;
using MicroServices.Account.Service.NpOn.IAccountService;
using MicroServices.Account.StorageAdapter.NpOn.AccountStorageAdapter;
using MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;

namespace MicroServices.Account.Service.NpOn.AccountService;

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
        // Main Database (account)
        // services.AddSingleton<IDbFactoryWrapper>(_ =>
        // {
        //     string connectionString =
        //         EApplicationConfiguration.ConnectionString.GetAppSettingConfig().AsDefaultString();
        //     int connectionNumber = EApplicationConfiguration.ConnectionNumber.GetAppSettingConfig().AsDefaultInt();
        //     IDbFactoryWrapper factoryWrapper =
        //         new DbFactoryWrapper(connectionString, EDb.Postgres, connectionNumber);
        //     return factoryWrapper;
        // });
        services.AddSingleton<IPostgresFactoryWrapper>(_ =>
        {
            string connectionString =
                EApplicationConfiguration.ConnectionString.GetAppSettingConfig().AsDefaultString();
            int connectionNumber =
                EApplicationConfiguration.ConnectionNumber.GetAppSettingConfig().AsDefaultInt();
            IDbFactoryWrapper factoryWrapper =
                new DbFactoryWrapper(connectionString, EDb.Postgres, connectionNumber);
            return new PostgresFactoryWrapper(factoryWrapper);
        });

        services.AddSingleton<IPostgresBaseRepository, PostgresBaseRepository>();

        services.AddSingleton<IRedisFactoryWrapper, RedisFactoryWrapper>(_ =>
        {
            string connectionString =
                EApplicationConfiguration.RedisConnectString.GetAppSettingConfig().AsDefaultString();
            int connectionNumber = EApplicationConfiguration.RedisConnectionNumber.GetAppSettingConfig().AsDefaultInt();
            IRedisFactoryWrapper factoryWrapper =
                new RedisFactoryWrapper(connectionString, EDb.Redis, connectionNumber, true);
            return (RedisFactoryWrapper)factoryWrapper;
        });

        if (EApplicationConfiguration.IsStartAsync.GetAppSettingConfig().AsDefaultBool())
        {
            services.AddHostedService<HostingApp>();
        }

        // rabbitMq
        bool isUseRabbitMq = EApplicationConfiguration.IsUseRabbitMq.GetAppSettingConfig().AsDefaultBool();
        if (isUseRabbitMq)
        {
            services.AddTransient<AccountSaveLoginRabbitMqConsumer>()
                .AddHostedService<ConsumerHostedService<AccountSaveLoginRabbitMqConsumer>>();
            services.AddTransient<AccountSaveLogoutRabbitMqConsumer>()
                .AddHostedService<ConsumerHostedService<AccountSaveLogoutRabbitMqConsumer>>();
        }

        // kafka
        bool isUseKafka = EApplicationConfiguration.IsUseKafka.GetAppSettingConfig().AsDefaultBool();
        if (isUseKafka)
        {
            services.AddTransient<AccountSaveLoginKafkaConsumer>()
                .AddHostedService<ConsumerHostedService<AccountSaveLoginKafkaConsumer>>();
        }

        // Add Service
        services.AddTransient<IAccountInfoService, AccountInfoService>();
        services.AddTransient<IAuthenticationService, AuthenticationService>();
        services.AddTransient<IAccountPermissionService, AccountPermissionService>();
        services.AddTransient<IAccountGroupService, AccountGroupService>();
        services.AddTransient<IAccountMenuService, AccountMenuService>();
        services.AddTransient<IAccountMenuPermissionService, AccountMenuPermissionService>();
        
        // Add Repository
        services.AddTransient<IAccountInfoStorageAdapter, AccountInfoStorageAdapter>();
        services.AddTransient<IAccountPermissionStorageAdapter, AccountPermissionStorageAdapter>();
        services.AddTransient<IAuthenticationStorageAdapter, AuthenticationStorageAdapter>();
        services.AddTransient<IAccountTokenAndPermissionRedisRepository, AccountTokenAndPermissionRedisRepository>();
        services.AddTransient<IAccountGroupStorageAdapter, AccountGroupStorageAdapter>();
        services.AddTransient<IAccountMenuStorageAdapter, AccountMenuStorageAdapter>();
        services.AddTransient<IAccountMenuPermissionStorageAdapter, AccountMenuPermissionStorageAdapter>();

        return Task.CompletedTask;
    }

    protected override void ConfigureBasePipeline(WebApplication app)
    {
        app.MapGet("/", () => "NpOn.AccountService");
        base.ConfigureBasePipeline(app);
    }

    protected override Task ConfigurePipeline(WebApplication app)
    {
        // Add Map Grpc Service
        app.MapGrpcService<AccountInfoService>();
        app.MapGrpcService<AuthenticationService>();
        app.MapGrpcService<AccountPermissionService>();
        app.MapGrpcService<AccountGroupService>();
        app.MapGrpcService<AccountMenuService>();
        app.MapGrpcService<AccountMenuPermissionService>();
        return Task.CompletedTask;
    }
}