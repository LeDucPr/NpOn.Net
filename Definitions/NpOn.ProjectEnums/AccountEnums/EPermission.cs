using System.ComponentModel.DataAnnotations;

namespace Definitions.NpOn.ProjectEnums.AccountEnums;

[Flags]
public enum EPermission
{
    [Display(Name = "Unknown")] Unknown = 0,

    [Display(Name = "Administrator")] Administrator = 1 << 0,
    
    [Display(Name = "Professor")] Professor = 1 << 1,
    [Display(Name = "SuperUser")] SuperUser = 1 << 1,

    [Display(Name = "Master")] Master = 1 << 2,
    [Display(Name = "MasterUser")] MasterUser = 1 << 2,
    
    [Display(Name = "Doctor")] Doctor = 1 << 3,
    [Display(Name = "StandardUser")] StandardUser = 1 << 3,
    
    [Display(Name = "Patient")] Patient = 1 << 4,
    [Display(Name = "User")] User = 1 << 4,
    
}

public enum EPermissionAccessController
{
    [Display(Name = "Enable")] Enable = 1,
    [Display(Name = "Disable")] Disable = 2,
}