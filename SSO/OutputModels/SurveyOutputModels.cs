namespace SSO.OutputModels;

public class QuestionsBySurveyModel
{
    public string? QuestionText { get; set; }
    public string? QuestionType { get; set; }
    public int QuestionOrder { get; set; }
    public bool IsRequired { get; set; }
    public int MaxScore { get; set; }
}