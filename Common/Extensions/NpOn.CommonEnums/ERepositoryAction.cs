using System.ComponentModel.DataAnnotations;

namespace Common.Extensions.NpOn.CommonEnums;

public enum ERepositoryAction
{
    [Display(Name = "Add")] Add = 1, 
    [Display(Name = "Update")] Update = 2, 
    [Display(Name = "Merge")] Merge = 3, 
    [Display(Name = "Delete")] Delete = 4, 
}