using System.ComponentModel.DataAnnotations;

namespace Definitions.NpOn.ProjectEnums.AccountEnums;

[Flags]
public enum ELoginType
{
    [Display(Name = "Default")] Default = 1,
}