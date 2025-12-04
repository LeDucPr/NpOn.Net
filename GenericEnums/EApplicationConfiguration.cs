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
    [Display(Name = "RedisConnectString")] RedisConnectString, // - string
    [Display(Name = "ConnectionNumber")] ConnectionNumber, // - int
    [Display(Name = "RedisConnectionNumber")] RedisConnectionNumber, // - int

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
    [Display(Name = "EventHandlerFilterQueues")] EventHandlerFilterQueues, // string
    [Display(Name = "RabbitMqExChangeNotifyListen")] RabbitMqExChangeNotifyListen, // string[]
    [Display(Name = "WorkerGroup")] WorkerGroup, // string[]
    #endregion RabbitMq
    
    
    #region Authen + Token
    [Display(Name = "JwtTokensKey")] JwtTokensKey, // - string
    [Display(Name = "CookieAuthenName")] CookieAuthenName, // - string
    [Display(Name = "CookieDomain")] CookieDomain, // - string
    [Display(Name = "AccountManagerAutomaticKeyGeneration")] AccountManagerAutomaticKeyGeneration, // - bool
    [Display(Name = "LoginExpiresTime")] LoginExpiresTime, // int -- 480 default
    #endregion Authen + Token
    
    
    #region Middleware
    [Display(Name = "IsUseMiddlewareLogger")] IsUseMiddlewareLogger, // - boolean
    #endregion Middleware 
    
    
    #region App - Controller - Service
    [Display(Name = "AppName")] AppName, // - string
    #endregion App - Controller - Service
    
    
    #region Exception
    [Display(Name = "ExceptionUrl")] ExceptionUrl, // - string
    [Display(Name = "UnauthenticatedAccountUrl")] UnauthenticatedAccountUrl, // - string
    #endregion Exception 
    
    
    #region UrlZone
    AccountServiceUrl, // - string
    GeneralServiceUrl, // - string
    QuestionServiceUrl, // - string
    #endregion UrlZone
}