using System.ComponentModel.DataAnnotations;

namespace Enums;

/// <summary>
/// EApplicationConfiguration get config params from appsettings.json
/// Đm cấm format
/// </summary>
public enum EApplicationConfiguration
{
    [Display(Name = "HostPort")] HostPort, // - int
    [Display(Name = "HostDomain")] HostDomain, // - string
    [Display(Name = "ConnectionString")] ConnectionString, // - string
    [Display(Name = "ConnectionNumber")] ConnectionNumber, // - int

    [Display(Name = "DnsResolverFactory")]
    DnsResolverFactory, // thời gian làm mới Dns service discovery (gRPC client-side load balancing) - int
    [Display(Name = "IsStartAsync")] IsStartAsync, // - boolean
    [Display(Name = "IsAutomaticKeyGeneration")] IsAutomaticKeyGeneration, // - boolean
    [Display(Name = "CorsPolicy")] CorsPolicy, // - string
    // ReSharper disable once InconsistentNaming
    [Display(Name = "CORS")] CORS, // - string
    [Display(Name = "KeepAlivePingDelaySeconds")] KeepAlivePingDelaySeconds, // - int
    [Display(Name = "KeepAlivePingTimeoutSeconds")] KeepAlivePingTimeoutSeconds, // - int
    [Display(Name = "IsDevEnvironment")] IsDevEnvironment, // - boolean
    
    #region RabbitMq
    [Display(Name = "IsUseRabbitMq")] IsUseRabbitMq, // bool
    
    [Display(Name = "RabbitMqPoolSizes")] RabbitMqPoolSize, // int
    [Display(Name = "RabbitMqHost")] RabbitMqHost, // string
    [Display(Name = "RabbitMqUserName")] RabbitMqUserName, // string
    [Display(Name = "RabbitMqPassword")] RabbitMqPassword, // string
    [Display(Name = "RabbitMqExchange")] RabbitMqExchange, // string
    [Display(Name = "RabbitMqRoutingRoot")] RabbitMqRoutingRoot, // string
    [Display(Name = "RabbitMqRouting")] RabbitMqRouting, // string
    [Display(Name = "RabbitMqQueues")] RabbitMqQueues, // string
    [Display(Name = "RabbitMqExchangeNotify")] RabbitMqExchangeNotify, // string
    [Display(Name = "VirtualHost")] VirtualHost, // string
    [Display(Name = "RabbitMqExchangesTrigger")] RabbitMqExchangesTrigger, // string
    #endregion RabbitMq
    
    #region UrlZone
    TZoneCallTestServiceUrl,
    TZoneServiceUrl,
    #endregion UrlZone
}