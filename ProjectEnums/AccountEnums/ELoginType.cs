using System.ComponentModel.DataAnnotations;

namespace ProjectEnums.AccountEnums;

[Flags]
public enum ELoginType
{
    [Display(Name = "Default")] Default = 1,
}