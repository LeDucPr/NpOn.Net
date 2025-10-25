using System.ComponentModel.DataAnnotations;

namespace Enums;

public enum EWebApplicationError
{
    [Display(Name = "HostDomain is not configured.")] HostDomain,
}