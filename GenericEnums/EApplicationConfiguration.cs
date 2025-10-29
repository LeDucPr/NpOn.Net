using System.ComponentModel.DataAnnotations;

namespace Enums;

public enum EApplicationConfiguration
{
    [Display(Name = "HostPort")] HostPort,
    [Display(Name = "HostDomain")] HostDomain,
}