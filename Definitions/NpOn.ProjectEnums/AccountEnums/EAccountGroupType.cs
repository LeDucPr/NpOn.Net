using System.ComponentModel.DataAnnotations;

namespace Definitions.NpOn.ProjectEnums.AccountEnums;

[Flags]
public enum EAccountGroupType
{
    [Display(Name = "InvalidGroup")] InvalidGroup = 0,
    [Display(Name = "PermissionGroup")] PermissionGroup = 1 << 0,
}