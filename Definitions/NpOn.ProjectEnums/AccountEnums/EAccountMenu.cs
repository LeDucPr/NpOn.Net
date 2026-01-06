using System.ComponentModel.DataAnnotations;

namespace Definitions.NpOn.ProjectEnums.AccountEnums;

public enum MenuItemType
{
    [Display(Name = "Item")] Item = 0,
    [Display(Name = "Heading")] Heading = 1,
    [Display(Name = "Separator")] Separator = 2,
}
public enum MenuScope
{
    [Display(Name = "Sidebar")] Sidebar = 0,
    [Display(Name = "Header")] Header = 1,
}
public enum AccountMenuStatus
{
    [Display(Name = "Deleted")] Deleted = 0,
    [Display(Name = "Active")] Active = 1,
    [Display(Name = "Unactive")] Unactive = 2,
}