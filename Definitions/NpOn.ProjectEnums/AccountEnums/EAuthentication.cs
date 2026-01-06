using System.ComponentModel.DataAnnotations;

namespace Definitions.NpOn.ProjectEnums.AccountEnums;

[Flags]
public enum EAuthentication
{
    [Display(Name = "Mobile")] Mobile = 1,
    [Display(Name = "WebApp")] WebApp = 2,
}