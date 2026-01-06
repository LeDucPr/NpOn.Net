using System.ComponentModel.DataAnnotations;

namespace Definitions.NpOn.ProjectEnums.AccountEnums;

public enum ETokenStatus
{
    [Display(Name = "Inactive")] Inactive = 0, // đã hết hạn
    [Display(Name = "Active")] Active = 1, // đang hoạt động 
}