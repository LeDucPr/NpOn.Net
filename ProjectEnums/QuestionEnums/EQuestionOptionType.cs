using System.ComponentModel.DataAnnotations;

namespace ProjectEnums.QuestionEnums;

public enum EQuestionOptionType
{
    [Display(Name="Single")] Single,
    [Display(Name="Multiple")] Multiple,
    [Display(Name="Text")] Text,
}