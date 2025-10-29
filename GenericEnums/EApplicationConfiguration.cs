using System.ComponentModel.DataAnnotations;

namespace Enums;

public enum EApplicationConfiguration
{
    [Display(Name = "HostPort")] HostPort,
    [Display(Name = "HostDomain")] HostDomain,
    [Display(Name = "ConnectionString")] ConnectionString,
    [Display(Name = "ConnectionNumber")] ConnectionNumber,
}