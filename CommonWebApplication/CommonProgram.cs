using System.Text.Json.Serialization;
using CommonMode;
using CommonObject;
using CommonWebApplication.Builders;
using CommonWebApplication.Parameters;
using Enums;
using CommonWebApplication.Services;
using Grpc.Net.Client.Balancer;
using GrpcAddService;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProtoBuf.Grpc.Server;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using RabbitMqBroker;

namespace CommonWebApplication;

public abstract class CommonProgram
{
    protected readonly string[] Args;

    protected CommonProgram(string[] args)
    {
        Args = args;
    }

    public async Task RunAsync()
    {
        var builder = CreateDefaultBuilder(Args);
        builder.Configuration.InitGlobal();
        await builder.Services.AddCollectionServices(async (services) =>
        {
            ConfigureBaseServices(services);
            await ConfigureServices(services);
            return services;
        });

        var app = builder.Build();

        await app.AddAppConfig(async (appConfig) =>
        {
            ConfigureBasePipeline(appConfig);
            await ConfigurePipeline(appConfig);
            return appConfig;
        });
        await app.RunAsync(); // run
    }


    #region For Enable Overrid Methods

    /// <summary>
    /// Configures services that are common to all applications.
    /// </summary>
    protected virtual void ConfigureBaseServices(IServiceCollection services)
    {
        services.AddLogging(p => p.AddSerilog(Log.Logger)); // add Log
        services.AddHttpContextAccessor(); // accessor 
        services.AddSingleton<ILogAction, LogAction>(); // as log ??

        // cors
        string corsConfig = EApplicationConfiguration.CORS.GetAppSettingConfig().AsDefaultString();
        if (corsConfig.Length > 0)
        {
            services.AddCors(options =>
            {
                var configs = corsConfig.Split(',').Select(p => p.Trim()).ToArray();
                options.AddPolicy(EApplicationConfiguration.CorsPolicy.GetAppSettingConfig().AsDefaultString(),
                    policyBuilder => policyBuilder.WithOrigins(configs).AllowAnyHeader().AllowCredentials()
                );
            });
        }

        // authentication 
        services.AddTransient<AuthenticationToken>();

        // common controllers
        services.AddControllers(options => { })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
        services.AddResponseCompression();

        // common grpc
        services.AddCodeFirstGrpc(config =>
        {
            config.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.NoCompression;
            config.MaxReceiveMessageSize = int.MaxValue;
            config.MaxSendMessageSize = int.MaxValue;
            //config.Interceptors.Add<>();
        });
        services.RegisterGrpcClientLoadBalancing(); // add DI multi Services
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        int dnsRsvF = EApplicationConfiguration.DnsResolverFactory.GetAppSettingConfig().AsDefaultInt();
        services.AddSingleton<ResolverFactory>(new DnsResolverFactory(refreshInterval: TimeSpan.FromSeconds(dnsRsvF)));
        // services.AddGrpc();

        // rabbitMq
        bool isUseRabbitMq = EApplicationConfiguration.IsUseRabbitMq.GetAppSettingConfig().AsDefaultBool();
        if (isUseRabbitMq)
        {
            string rabbitMqHost = EApplicationConfiguration.RabbitMqHost.GetAppSettingConfig().AsDefaultString();
            if (rabbitMqHost.Length > 0)
            {
                string virtualHost =
                    EApplicationConfiguration.VirtualHost.GetAppSettingConfig().AsDefaultString();

                string rabbitMqUserName =
                    EApplicationConfiguration.RabbitMqUserName.GetAppSettingConfig().AsDefaultString();
                string rabbitMqPassword =
                    EApplicationConfiguration.RabbitMqPassword.GetAppSettingConfig().AsDefaultString();
                services.AddSingleton<RabbitMqConnectionPool>(sp =>
                {
                    ILogger<RabbitMqConnectionPool> logger = sp.GetRequiredService<ILogger<RabbitMqConnectionPool>>();
                    int poolSize = EApplicationConfiguration.RabbitMqPoolSize.GetAppSettingConfig().AsDefaultInt();
                    if (poolSize <= 0)
                        poolSize = 1; // default
                    return new RabbitMqConnectionPool(logger, poolSize, [rabbitMqHost], virtualHost,
                        rabbitMqUserName, rabbitMqPassword);
                });
            }

            if (EApplicationConfiguration.IsUseRabbitMq.GetAppSettingConfig().AsDefaultBool())
            {
                string rabbitMqExchange =
                    EApplicationConfiguration.RabbitMqExchange.GetAppSettingConfig().AsDefaultString();
                string rabbitMqRoutingRoot =
                    EApplicationConfiguration.RabbitMqRoutingRoot.GetAppSettingConfig().AsDefaultString();
                string rabbitMqRouting =
                    EApplicationConfiguration.RabbitMqRouting.GetAppSettingConfig().AsDefaultString();
                string rabbitMqQueues =
                    EApplicationConfiguration.RabbitMqQueues.GetAppSettingConfig().AsDefaultString();
                string rabbitMqExchangeNotify =
                    EApplicationConfiguration.RabbitMqExchangeNotify.GetAppSettingConfig().AsDefaultString();
                string rabbitMqExchangesTrigger = EApplicationConfiguration.RabbitMqExchangesTrigger
                    .GetAppSettingConfig().AsDefaultString();
                if (rabbitMqExchange.Length > 0 || rabbitMqExchangeNotify.Length > 0 ||
                    rabbitMqExchangesTrigger.Length > 0)
                {
                    services.AddSingleton<IRabbitMqEventProcessor, RabbitMqEventProcessor>(sp =>
                    {
                        var logger = sp.GetRequiredService<ILogger<RabbitMqEventProcessor>>();
                        var serviceProvider = sp.GetRequiredService<IServiceProvider>();
                        var connectionPool = sp.GetRequiredService<RabbitMqConnectionPool>();

                        // Đọc các giá trị cấu hình
                        string exchange = EApplicationConfiguration.RabbitMqExchange.GetAppSettingConfig()
                            .AsDefaultString();
                        string exchangeNotify = EApplicationConfiguration.RabbitMqExchangeNotify.GetAppSettingConfig()
                            .AsDefaultString();
                        string filterQueuesConfig = EApplicationConfiguration.EventHandlerFilterQueues
                            .GetAppSettingConfig().AsDefaultString().ToLower();
                        string[] routingKeys = EApplicationConfiguration.RabbitMqRouting.GetAppSettingConfig()
                            .AsDefaultString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                        string[] topics = EApplicationConfiguration.RabbitMqQueues.GetAppSettingConfig()
                            .AsDefaultString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                        string exchangeNotifyListen = EApplicationConfiguration.RabbitMqExChangeNotifyListen
                            .GetAppSettingConfig().AsDefaultString();
                        string[] exchangesTrigger = EApplicationConfiguration.RabbitMqExchangesTrigger
                            .GetAppSettingConfig().AsDefaultString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                        string[] workerGroup = EApplicationConfiguration.WorkerGroup.GetAppSettingConfig()
                            .AsDefaultString().ToLower().Split(',',
                                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                        return new RabbitMqEventProcessor(logger, serviceProvider, connectionPool, exchange,
                            exchangeNotify,
                            filterQueuesConfig, routingKeys, topics, exchangeNotifyListen, exchangesTrigger,
                            workerGroup);
                    });

                    services.AddHostedService<RabbitMqBusStarter>();
                }
            }
        }
    }

    /// <summary>
    /// Configures services specific to the derived application.
    /// </summary>
    protected abstract Task ConfigureServices(IServiceCollection services);

    /// <summary>
    /// Configures the common parts of the HTTP request pipeline.
    /// </summary>
    protected virtual void ConfigureBasePipeline(WebApplication app)
    {
        app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        if (this.GetType() == typeof(CommonProgram))
            throw new Exception($"{nameof(CommonProgram.ConfigureBasePipeline)} need override.");
    }

    /// <summary>
    /// Configures the HTTP request pipeline specific to the derived application (e.g., mapping gRPC services).
    /// </summary>
    protected abstract Task ConfigurePipeline(WebApplication app);

    #endregion For Enable Overrid Methods


    #region Private Methods

    private WebApplicationBuilder CreateDefaultBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // host-domain-start
        string hostDomain = builder.Configuration.TryGetConfig(EApplicationConfiguration.HostDomain).AsDefaultString();
        var hostPort = builder.Configuration.TryGetConfig(EApplicationConfiguration.HostPort).AsDefaultInt();
        if (hostPort > 0)
            hostDomain = $"{hostDomain}:{hostPort}";
        if (string.IsNullOrWhiteSpace(hostDomain))
            throw new Exception(EWebApplicationError.HostDomain.GetDisplayName());
        builder.WebHost.UseUrls($"{hostDomain}:{hostPort}");
        return builder;
    }

    #endregion Private Methods
}