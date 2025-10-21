using System.ComponentModel.DataAnnotations;

namespace Enums;

public enum EDbJoinType
{
    [Display(Name = "Join")] Join = 1,
    [Display(Name = "Inner Join")] InnerJoin = 1,

    [Display(Name = "Left Outer Join")] LeftOuterJoin = 2,
    [Display(Name = "Left Outer Join")] LeftJoin = 2,
    [Display(Name = "Right Outer Join")] RightOuterJoin = 2,
    [Display(Name = "Right Outer Join")] RightJoin = 2,

    [Display(Name = "Full Outer Join")] FullJoin = 3,
    [Display(Name = "Full Outer Join")] FullOuterJoin = 3,

    [Display(Name = "Cross Join")] CrossJoin = 4, // Descartes
    [Display(Name = "Self Join")] SelfJoin = 5 // ?? use when ??
}
