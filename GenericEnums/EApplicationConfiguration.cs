using System.ComponentModel.DataAnnotations;

namespace Enums;

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
}