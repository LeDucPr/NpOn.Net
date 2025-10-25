using System.ComponentModel.DataAnnotations;

namespace BaseWebApplication.Parameters;

public enum EConfiguration
{
    [Display(Name = "HostPort")] HostPort,
    [Display(Name = "HostDomain")] HostDomain,
}