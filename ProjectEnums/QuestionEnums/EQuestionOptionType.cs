using System.ComponentModel.DataAnnotations;

namespace ProjectEnums.QuestionEnums;

[Flags]
public enum EQuestionOptionType
{
    [Display(Name="Single")] Single = 1,
    [Display(Name="Multiple")] Multiple = 2,
    [Display(Name="Text")] Text = 4,
}