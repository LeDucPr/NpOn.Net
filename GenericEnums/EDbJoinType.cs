using System.ComponentModel.DataAnnotations;

namespace Enums;

[Flags]
public enum EDbJoinType
{
    [Display(Name = "Join")] Join = 1,
    [Display(Name = "Inner Join")] InnerJoin = 1,

    [Display(Name = "Left Outer Join")] LeftOuterJoin = 2,
    [Display(Name = "Left Outer Join")] LeftJoin = 2,
    [Display(Name = "Right Outer Join")] RightOuterJoin = 4,
    [Display(Name = "Right Outer Join")] RightJoin = 4,

    [Display(Name = "Full Outer Join")] FullJoin = 8,
    [Display(Name = "Full Outer Join")] FullOuterJoin = 8,

    [Display(Name = "Cross Join")] CrossJoin = 16, // Descartes
    [Display(Name = "Self Join")] SelfJoin = 32, // ?? use when ??
}
