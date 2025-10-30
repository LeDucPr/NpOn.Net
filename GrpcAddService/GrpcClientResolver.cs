using CommonMode;
using CommonObject;
using Enums;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Grpc.Net.ClientFactory;
using GrpcAddService.Components;
using ProtoBuf.Grpc.ClientFactory;

namespace GrpcAddService;

public static class GrpcClientResolver
{
    public static IServiceCollection RegisterGrpcClientLoadBalancing(this IServiceCollection services)
    {
        // Register the interceptor so the DI container knows how to create it.
        services.AddTransient<LoggerInterceptor>();
        
        services.ZoneServiceRegisterGrpc();
        services.ZoneCallTestServiceRegisterGrpc();
        return services;
    }

    private static void ConfigGrpcClientOptions(GrpcClientFactoryOptions grpcClientFactoryOptions, string address,
        IServiceProvider serviceProvider)
    {
        // if (!address.StartsWith("http"))
        // {
        //     return;
        // }

        SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler()
        {
            // keeps connection alive
            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
            KeepAlivePingDelay =
                TimeSpan.FromSeconds(
                    EApplicationConfiguration.KeepAlivePingDelaySeconds.GetAppSettingConfig().AsDefaultInt()),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(
                EApplicationConfiguration.KeepAlivePingTimeoutSeconds.GetAppSettingConfig().AsDefaultInt()),
            EnableMultipleHttp2Connections = true, // allows channel to add additional HTTP/2 connections
            MaxConnectionsPerServer = int.MaxValue,
        };
        var methodConfig = new MethodConfig
        {
            Names = { MethodName.Default },
            RetryPolicy = new RetryPolicy
            {
                MaxAttempts = 5,
                InitialBackoff = TimeSpan.FromSeconds(1),
                MaxBackoff = TimeSpan.FromSeconds(5),
                BackoffMultiplier = 1.5,
                RetryableStatusCodes = { StatusCode.Unavailable }
            }
        };
        grpcClientFactoryOptions.Address = new Uri(address);
        grpcClientFactoryOptions.ChannelOptionsActions.Add(o =>
        {
            o.HttpHandler = socketsHttpHandler;
            o.MaxReceiveMessageSize = int.MaxValue;
            o.MaxSendMessageSize = int.MaxValue;
            o.Credentials = ChannelCredentials.Insecure;
            o.ServiceProvider = serviceProvider;
            o.ServiceConfig = new ServiceConfig
            {
                LoadBalancingConfigs = { new RoundRobinConfig() },
                MethodConfigs = { methodConfig }
            };
        });
    }

    public static IServiceCollection RegisterGrpcClientLoadBalancing<T>(this IServiceCollection services, string? url)
        where T : class
    {
        if (url is not { Length: > 0 })
        {
            return services;
        }

        services.AddCodeFirstGrpcClient<T>((provider, options) => { ConfigGrpcClientOptions(options, url, provider); })
            .AddInterceptor(provider =>
            {
                LoggerInterceptor loggerInterceptor = provider.GetRequiredService<LoggerInterceptor>();
                loggerInterceptor.Host = url;
                return loggerInterceptor;
            });
        return services;
    }

    public static IServiceCollection RegisterGrpcClientLoadBalancing<T>(this IServiceCollection services, string? url,
        string? name)
        where T : class
    {
        if (url is not { Length: > 0 })
        {
            return services;
        }

        if (name?.Length > 0)
        {
            services.AddCodeFirstGrpcClient<T>(name,
                    (provider, options) => { ConfigGrpcClientOptions(options, url, provider); })
                .AddInterceptor(provider =>
                {
                    LoggerInterceptor loggerInterceptor = provider.GetRequiredService<LoggerInterceptor>();
                    loggerInterceptor.Host = url;
                    return loggerInterceptor;
                });
        }
        else
        {
            RegisterGrpcClientLoadBalancing<T>(services, url);
        }

        return services;
    }
}