using System.ComponentModel.DataAnnotations;

namespace ProjectEnums.AccountEnums;

[Flags]
public enum EPermission
{
    [Display(Name = "Unknown")] Unknown = 0,
    [Display(Name = "Administrator")] Administrator = 1 << 0,
    [Display(Name = "SuperUser")] SuperUser = 1 << 1,
    [Display(Name = "User")] User = 1 << 2,
}