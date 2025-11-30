namespace QuestionServiceObject.QueryObjects;

public class SurveyHistoryQuery
{
    public string? ResultId { get; set; }

    public string? UserId { get; set; }

    public string? SurveyId { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }
}