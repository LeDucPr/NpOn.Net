using System.Net;
using System.Security.Cryptography;
using System.Text;
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
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using RabbitMqExtMs.Generics;
using RabbitMqExtMs.Senders;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace CommonWebApplication;

public abstract class CommonProgram
{
    protected readonly string[] Args;

    protected CommonProgram(string[] args)
    {
        Args = args;
    }

    protected async Task RunAsync()
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
        services.AddControllers(_ => { })
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
            string rabbitCnStr = EApplicationConfiguration.RabbitMqConnection.GetAppSettingConfig().AsDefaultString();
            string exName = EApplicationConfiguration.RabbitMqExchangeName.GetAppSettingConfig().AsDefaultString();
            RabbitMqConnection rabbitMqConnection = new RabbitMqConnection(rabbitCnStr, exName);
            services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>(_ => rabbitMqConnection);
            services.AddTransient<IRabbitMqProducer, RabbitMqProducer>(_ => new RabbitMqProducer(rabbitMqConnection));
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
                var jwtKey = EApplicationConfiguration.JwtTokensKey.GetAppSettingConfig().AsDefaultString();
                var key = Encoding.ASCII.GetBytes(jwtKey);
                options.RequireHttpsMetadata = false; // Chỉ dùng false trong môi trường dev
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // Tùy chỉnh nếu bạn có issuer cụ thể
                    ValidateAudience = false // Tùy chỉnh nếu bạn có audience cụ thể
                    // ValidateIssuer = true,
                    // ValidateAudience = true,
                    // ValidateLifetime = true,
                    // ValidateIssuerSigningKey = true,
                    // ValidIssuer = "your_issuer",
                    // ValidAudience = "your_audience",
                    // IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_super_secret_key"))
                };
            })
            .AddPolicyScheme("BearerOrCookie", "Bearer or Cookie", options =>
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

    /// <summary>
    /// Configures services specific to the derived application.
    /// </summary>
    protected abstract Task ConfigureServices(IServiceCollection services);

    /// <summary>
    /// Configures the common parts of the HTTP request pipeline.
    /// </summary>
    protected virtual void ConfigureBasePipeline(WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseRouting();

        // Activate the CORS policy that was configured in ConfigureBaseServices
        string corsPolicy = EApplicationConfiguration.CorsPolicy.GetAppSettingConfig().AsDefaultString();
        if (!string.IsNullOrWhiteSpace(corsPolicy))
        {
            app.UseCors(corsPolicy);
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        string appName = EApplicationConfiguration.AppName.GetAppSettingConfig().AsDefaultString();
        app.MapGet("/", () => appName);
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