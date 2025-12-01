using System.ComponentModel.DataAnnotations;

namespace ProjectEnums.AccountEnums;

public enum EAccountStatus
{
    [Display(Name = "Active")] Deleted = 0,
    [Display(Name = "Active")] Active = 1,
    [Display(Name = "Unactive")] Unactive = 2,
}