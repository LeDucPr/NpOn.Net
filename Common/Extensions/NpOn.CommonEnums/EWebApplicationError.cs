using System.ComponentModel.DataAnnotations;

namespace Common.Extensions.NpOn.CommonEnums;

public enum EWebApplicationError
{
    [Display(Name = "HostDomain is not configured.")] HostDomain,
}