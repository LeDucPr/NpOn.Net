using System.ComponentModel.DataAnnotations;

namespace Definitions.NpOn.ProjectEnums.AccountEnums;

public enum EAccountStatus
{
    [Display(Name = "Active")] Deleted = 0,
    [Display(Name = "Active")] Active = 1,
    [Display(Name = "Unactive")] Unactive = 2,
}

public enum EAccountInfoStatus
{
    [Display(Name = "Active")] Deleted = 0,
    [Display(Name = "Active")] Active = 1,
    [Display(Name = "Unactive")] Unactive = 2,
}

public enum EAccountGender
{
    [Display(Name = "Unknown")] Unknown = 0,
    [Display(Name = "Male")] Male = 1,
    [Display(Name = "Female")] Female = 2,
}