namespace SSO.Requests;

public record SubmitSurveyRequest
{
    public required string SurveyId { get; set; }

    public required List<SurveyAnswerRequest> Answers { get; set; }
}

public record SurveyAnswerRequest
{
    public required string QuestionId { get; set; }

    public List<string>? SelectedOptionIds { get; set; }

    public string? TextAnswer { get; set; }
    
    public int? ScoreTextAnswer { get; set; }
}