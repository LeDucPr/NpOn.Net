using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication.Builders;
using Common.Extensions.NpOn.CommonWebApplication.Parameters;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Common.Infrastructures.NpOn.KafkaExtCm.Configs;
using Common.Infrastructures.NpOn.KafkaExtCm.Senders;
using Common.Infrastructures.NpOn.KafkaExtCm.Topics;
using Common.Infrastructures.NpOn.RabbitMqExtMs.Generics;
using Common.Infrastructures.NpOn.RabbitMqExtMs.Senders;
using Confluent.Kafka;
using Definitions.NpOn.GrpcAddService;
using Grpc.Net.Client.Balancer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using ProtoBuf.Grpc.Server;
using Serilog;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Common.Extensions.NpOn.CommonWebApplication;

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

        // [FIX] Configure ForwardedHeaders to correctly identify the Protocol (HTTPS) when running behind a Reverse Proxy (Docker/Nginx)
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear(); // Trust internal docker networks
            options.KnownProxies.Clear();
        });

        // cors
        string corsConfig = EApplicationConfiguration.CORS.GetAppSettingConfig().AsDefaultString();
        if (corsConfig.Length > 0)
        {
            services.AddCors(options =>
            {
                string[] configs = corsConfig
                    .Split(',')
                    .Select(p => p.Trim())
                    .ToArray();
                string autoAddCredential =
                    EApplicationConfiguration.AutoAddCredential.GetAppSettingConfig().AsDefaultString();
                if (autoAddCredential is { Length : > 0 })
                    configs = configs.Select(p => p.StartsWith(autoAddCredential) ? p : $"{autoAddCredential}://" + p)
                        .ToArray();

                options.AddPolicy(EApplicationConfiguration.CorsPolicy.GetAppSettingConfig().AsDefaultString(),
                    policyBuilder =>
                    {
                        // If config contains "*" -> Allow all Origins (including file://) + Credentials
                        if (configs.Contains("*"))
                        {
                            // [FIX] Use SetIsOriginAllowed to allow 'null' origin (local file) and others dynamically
                            policyBuilder.SetIsOriginAllowed(_ => true)
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials()
                                .WithExposedHeaders("Upload-Offset", "Location", "Upload-Length", "Tus-Version",
                                    "Tus-Resumable", "Tus-Max-Size", "Tus-Extension", "Upload-Metadata",
                                    "Upload-Defer-Length", "Upload-Concat", "X-Media-Download-Url");
                        }
                        else
                        {
                            policyBuilder.WithOrigins(configs)
                                .AllowAnyHeader()
                                .AllowCredentials()
                                .AllowAnyMethod()
                                .WithExposedHeaders("Upload-Offset", "Location", "Upload-Length", "Tus-Version",
                                    "Tus-Resumable", "Tus-Max-Size", "Tus-Extension", "Upload-Metadata",
                                    "Upload-Defer-Length", "Upload-Concat", "X-Media-Download-Url");
                        }
                    }
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
        services.AddSingleton<PermissionService>();

        // common controllers
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new GuidEmptyAsNullConverter());
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
            // Connection and Producer of RabbitMQ must be Singleton to keep TCP connection
            services.AddSingleton<IRabbitMqConnection>(rabbitMqConnection);
            services.AddSingleton<IRabbitMqProducer, RabbitMqProducer>();
        }

        // kafka
        bool isUseKafka = EApplicationConfiguration.IsUseKafka.GetAppSettingConfig().AsDefaultBool();
        if (isUseKafka)
        {
            string kafkaCnStr = EApplicationConfiguration.KafkaConnection.GetAppSettingConfig().AsDefaultString();
            string topicName = EApplicationConfiguration.KafkaTopicName.GetAppSettingConfig().AsDefaultString();
            // kafka auth
            string saslUsername = EApplicationConfiguration.SaslUsername.GetAppSettingConfig().AsDefaultString();
            string saslPassword = EApplicationConfiguration.SaslPassword.GetAppSettingConfig().AsDefaultString();

            KafkaClientConfigBuilder configBuilder = new KafkaClientConfigBuilder();
            configBuilder.SetServerUrl(kafkaCnStr);
            if (!string.IsNullOrEmpty(saslUsername) && !string.IsNullOrEmpty(saslPassword))
                // SASL authentication (currently only this mechanism is supported =)))
                configBuilder.SetUseSasl(saslUsername, saslPassword, SaslMechanism.ScramSha256);
            KafkaTopic kafkaTopic = KafkaTopic.Create(configBuilder.Build(), topicName)
                .GetAwaiter()
                .GetResult();
            services.AddSingleton<IKafkaTopic>(kafkaTopic);
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
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
                options.DefaultScheme = "BearerOrCookie";
                options.DefaultChallengeScheme = "BearerOrCookie";
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy =
                    EApplicationConfiguration.IsDevEnvironment.GetAppSettingConfig().AsDefaultBool()
                        ? CookieSecurePolicy.SameAsRequest
                        : CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Unspecified;
                options.Cookie.Name =
                    EApplicationConfiguration.CookieAuthenName.GetAppSettingConfig().AsDefaultString();
                options.LoginPath = string.Empty; //"api/Account/Login";
                options.LogoutPath = string.Empty; //"api/Account/Logout";
                options.AccessDeniedPath = string.Empty;
                string cookieDomain =
                    EApplicationConfiguration.CookieDomain.GetAppSettingConfig().AsDefaultString();
                if (cookieDomain.Length > 0)
                {
                    options.Cookie.Domain = cookieDomain;
                }

                options.Events.OnRedirectToLogin = context =>
                {
                    // Always return 401 Unauthorized for API instead of Redirect (to avoid CORS errors or returning an HTML login page)
                    // Since LoginPath is empty, returning 401 is the most appropriate behavior for both Dev and Prod.
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Task.CompletedTask;
                };
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var jwtKey = EApplicationConfiguration.JwtTokensKey.GetAppSettingConfig().AsDefaultString();
                var key = Encoding.ASCII.GetBytes(jwtKey);
                options.RequireHttpsMetadata = false; // Use false only in dev environment
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // Customize if you have a specific issuer
                    ValidateAudience = false // Customize if you have a specific audience
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
        var keyLinuxPath = "/home/app/.aspnet/DataProtection-Keys";
        if (!Directory.Exists(keyLinuxPath))
        {
            Directory.CreateDirectory(keyLinuxPath);
        }

        string appName = EApplicationConfiguration.AppName.GetAppSettingConfig().AsDefaultString();
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keyLinuxPath))
            .SetApplicationName(appName);
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
        // [FIX] Phải đặt đầu pipeline để xác định đúng Scheme (HTTP/HTTPS) trước khi Authentication chạy
        app.UseForwardedHeaders();

        app.UseRouting();

        // CORS middleware must be placed after UseRouting and before UseAuthentication/UseAuthorization
        // to correctly handle preflight OPTIONS requests.
        string corsPolicy = EApplicationConfiguration.CorsPolicy.GetAppSettingConfig().AsDefaultString();
        if (!string.IsNullOrWhiteSpace(corsPolicy))
        {
            app.UseCors(corsPolicy);
        }

        app.UseHttpsRedirection();
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

public class GuidEmptyAsNullConverter : System.Text.Json.Serialization.JsonConverter<Guid?>
{
    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value == null || value == Guid.Empty)
        {
            // null → JSON engine will be ignored property WhenWritingNull
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value);
    }

    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetGuid();
    }
}