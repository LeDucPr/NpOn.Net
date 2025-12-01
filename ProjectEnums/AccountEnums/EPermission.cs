using System.ComponentModel.DataAnnotations;

namespace ProjectEnums.AccountEnums;

[Flags]
public enum EPermission
{
    [Display(Name = "Administrator")] Administrator = 1 << 0,
    [Display(Name = "Doctor")] Doctor = 1 << 1,
    [Display(Name = "Patient")] Patient = 1 << 2, 
}