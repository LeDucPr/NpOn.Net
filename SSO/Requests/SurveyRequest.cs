namespace SSO.Requests;

public record QuestionGetBySurveyIdRequest
{
    public required string SurveyId { get; set; }
}