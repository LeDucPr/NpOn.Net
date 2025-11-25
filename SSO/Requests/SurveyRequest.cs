namespace SSO.Requests;

public record QuestionGetBySurveyIdRequest
{
    public required string SurveyId { get; set; }
}

public record SubmitSurveyRequest
{
    public required string SurveyId { get; set; }
    public string? UserId { get; set; }
    public required List<SubmitAnswerRequest> Answers { get; set; }
}

public record SubmitAnswerRequest
{
    public required string QuestionId { get; set; }
    public string? TextAnswer { get; set; }
    public List<string>? SelectedOptionIds { get; set; }
}

public record CalculateSurveyScoreRequest
{
    public required string SurveyId { get; set; }
    public required List<SubmitAnswerRequest> Answers { get; set; }
}