using System.ComponentModel.DataAnnotations;

namespace Definitions.NpOn.ProjectEnums.AccountEnums;

public enum EAddressType
{
    [Display(Name = "Other")] Other = 0, 
    [Display(Name = "Default")] Default = 1, 
}