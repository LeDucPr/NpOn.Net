using System.Net;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using CommonMode;
using CommonObject;
using CommonWebApplication.Builders;
using CommonWebApplication.Parameters;
using Enums;
using CommonWebApplication.Services;
using Grpc.Net.Client.Balancer;
using GrpcAddService;
using ProtoBuf.Grpc.Server;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.IdentityModel.Logging;
using Microsoft.Net.Http.Headers;
using RabbitMqBroker;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

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
        services.AddHttpContextAccessor(); // accessor 

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

        // logger
        services.AddSingleton<ILogAction, LogAction>(); // as log ??

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // Ensure the general minimum is low enough
            .WriteTo.Console()
            .WriteTo.File(
                path: $"logs/log-{DateTime.Now:yyyyMMdd_HHmmss}.txt", // start time
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information // Information
            )
            .CreateLogger();

        services.AddLogging(p => p.AddSerilog(Log.Logger)); // add log (console)

        // authentication 
        services.AddTransient<AuthenticationToken>();
        services.AddTransient<ContextService>();
        services.AddTransient<AuthenService>();

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

            ////// Controller
            // authorization 
            services.AddAuthorization();
            // authorization policy
            services.AddAuthorization(options =>
            {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme,
                    CookieAuthenticationDefaults.AuthenticationScheme);
                defaultAuthorizationPolicyBuilder =
                    defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });

            // authentication
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Unspecified;
                    options.Cookie.Name =
                        EApplicationConfiguration.CookieAuthenName.GetAppSettingConfig().AsDefaultString();
                    options.LoginPath = string.Empty;
                    options.LogoutPath = string.Empty;
                    options.AccessDeniedPath = string.Empty;
                    string cookieDomain =
                        EApplicationConfiguration.CookieDomain.GetAppSettingConfig().AsDefaultString();
                    if (cookieDomain.Length > 0)
                    {
                        options.Cookie.Domain = cookieDomain;
                    }
#if DEBUG
                    options.Events.OnRedirectToLogin = context =>
                    {
                        if (EApplicationConfiguration.IsDevEnvironment.GetAppSettingConfig().AsDefaultBool()) // debug
                        {
                            if (context.Request.Path.StartsWithSegments("/api") &&
                                context.Response.StatusCode == (int)HttpStatusCode.OK)
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            else
                                context.Response.Redirect(context.RedirectUri);
                        }

                        return Task.FromResult(0);
                    };
#endif
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    // Cấu hình các tham số xác thực token ở đây
                    // Ví dụ:
                    // options.TokenValidationParameters = new TokenValidationParameters
                    // {
                    //     ValidateIssuer = true,
                    //     ValidateAudience = true,
                    //     ValidateLifetime = true,
                    //     ValidateIssuerSigningKey = true,
                    //     ValidIssuer = "your_issuer",
                    //     ValidAudience = "your_audience",
                    //     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_super_secret_key"))
                    // };
                })
                .AddPolicyScheme("JwtBearer", "Cookie", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        string? authorization = context.Request.Headers[HeaderNames.Authorization];
                        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                            return JwtBearerDefaults.AuthenticationScheme;
                        return CookieAuthenticationDefaults.AuthenticationScheme;
                    };
                });
#if DEBUG
            if (EApplicationConfiguration.IsDevEnvironment.GetAppSettingConfig().AsDefaultBool()) // debug
                IdentityModelEventSource.ShowPII = true;
#endif
            IDataProtectionBuilder dataProtectionBuilder = services
                .AddDataProtection()
                .UseCustomCryptographicAlgorithms(
                    new ManagedAuthenticatedEncryptorConfiguration()
                    {
                        EncryptionAlgorithmType = typeof(Aes),
                        EncryptionAlgorithmKeySize = 256,
                        ValidationAlgorithmType = typeof(HMACSHA512)
                    });
            if (!EApplicationConfiguration.AccountManagerAutomaticKeyGeneration.GetAppSettingConfig().AsDefaultBool())
            {
                dataProtectionBuilder.DisableAutomaticKeyGeneration();
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