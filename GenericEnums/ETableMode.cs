using System.ComponentModel.DataAnnotations;

namespace Enums;

[Flags]
public enum ETableMode
{
    [Display(Name = "All")] All = 0,
    [Display(Name = "ReadOnly")] ReadOnly = 1 << 0,
    [Display(Name = "WriteEnable")] WriteEnable = 1 << 1,
    [Display(Name = "ViewMaterial")] ViewMaterial = 1 << 2,
    [Display(Name = "FromExecuteProcess")] FromExecuteProcess = 1 << 4,
}

public enum EFieldSize
{
    [Display(Name = "Limit")] Limit = 1,
    [Display(Name = "Unlimit")] Unlimit = 2,
}