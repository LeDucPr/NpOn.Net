namespace SSO.Requests;

public record GetSurveyHistoryRequest
{
    public string? ResultId { get; set; }

    public string? UserId { get; set; }

    public string? SurveyId { get; set; }

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}