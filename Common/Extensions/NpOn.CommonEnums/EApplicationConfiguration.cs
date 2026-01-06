using System.ComponentModel.DataAnnotations;

namespace Common.Extensions.NpOn.CommonEnums;

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
    [Display(Name = "IsUseRedisCache")] IsUseRedisCache, // - bool

    [Display(Name = "DnsResolverFactory")]
    DnsResolverFactory, // thời gian làm mới Dns service discovery (gRPC client-side load balancing) - int
    [Display(Name = "IsStartAsync")] IsStartAsync, // - boolean
    [Display(Name = "IsAutomaticKeyGeneration")] IsAutomaticKeyGeneration, // - boolean
    [Display(Name = "CorsPolicy")] CorsPolicy, // - string
    [Display(Name = "AutoAddCredential")] AutoAddCredential, // - string
    // ReSharper disable once InconsistentNaming
    [Display(Name = "CORS")] CORS, // - string
    [Display(Name = "KeepAlivePingDelaySeconds")] KeepAlivePingDelaySeconds, // - int
    [Display(Name = "KeepAlivePingTimeoutSeconds")] KeepAlivePingTimeoutSeconds, // - int
    [Display(Name = "IsDevEnvironment")] IsDevEnvironment, // - boolean
    
    
    #region RabbitMq
    [Display(Name = "IsUseRabbitMq")] IsUseRabbitMq, // bool
    [Display(Name = "RabbitMqConnection")] RabbitMqConnection, // string
    [Display(Name = "RabbitMqExchangeName")] RabbitMqExchangeName, // string
    #endregion RabbitMq
    
    
    #region Kafka
    [Display(Name = "IsUseKafka")] IsUseKafka, // bool
    [Display(Name = "KafkaConnection")] KafkaConnection, // string
    [Display(Name = "SaslUsername")] SaslUsername, // string
    [Display(Name = "SaslPassword")] SaslPassword, // string
    [Display(Name = "KafkaTopicName")] KafkaTopicName, // string
    #endregion Kafka
    
    
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
    #endregion UrlZone
    
    #region Media Upload
    [Display(Name = "MediaUploadEndpoint")] MediaUploadEndpoint, // - string (e.g. /api/Media/Upload)
    [Display(Name = "MediaTempStoragePath")] MediaTempStoragePath, // - string (e.g. D:\NpOn_Data\Tus_Temp)
    [Display(Name = "MediaPublicStoragePath")] MediaPublicStoragePath, // - string (e.g. D:\NpOn_Data\Public_Media)
    [Display(Name = "MediaDownloadUrlPrefix")] MediaDownloadUrlPrefix, // - string (e.g. /api/Media/Download)
    [Display(Name = "MediaDeleteTempOnSuccess")] MediaDeleteTempOnSuccess, // - bool (default true)
    [Display(Name = "IsMediaEnablePublicDownload")] IsMediaEnablePublicDownload, // - bool (default false)
    [Display(Name = "MediaAllowedExtensions")] MediaAllowedExtensions, // - string (e.g. .jpg,.png,.mp4) or *
    [Display(Name = "MediaMaxFileSize")] MediaMaxFileSize, // - long (bytes)
    [Display(Name = "IsMediaDeleteEnabled")] IsMediaDeleteEnabled, // - bool (default false)
    [Display(Name = "MediaCdnUrl")] MediaCdnUrl, // - string (e.g. https://cdn.example.com)
    #endregion Media Upload
}