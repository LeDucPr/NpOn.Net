namespace QuestionServiceObject.CommandObjects;

public class SurveyResultInsertCommand
{
    public required string UserId { get; set; }
    public required string SurveyId { get; set; }
    public long TotalScore { get; set; }
    public long MaxScore { get; set; }
    public required string OutcomeData { get; set; }
}