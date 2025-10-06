using System.ComponentModel.DataAnnotations;

namespace Enums;

public enum EHandlerError
{
    [Display(Name = "Dependent properties not null")] DependentProperty, 
    [Display(Name = "Inherit type is not correct")] RelatedType, 
    [Display(Name = "Navigation property can not initialize")] NavigationProperty
}