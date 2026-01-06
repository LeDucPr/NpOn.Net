namespace Controllers.NpOn.SSO.OutputModels;

public class SurveyModel
{
    public required string SurveyId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsPublished => true;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public QuestionSimpleModel[]? Questions { get; set; }
}

public class QuestionSimpleModel
{
    public required string QuestionId { get; set; }
    public required string SurveyId { get; set; }
    public required QuestionOptionModel Options { get; set; }
    public AnswerModel[]? Answers { get; set; } // null when setup to answer as text
    public string? QuestionText { get; set; }
    public int QuestionOrder { get; set; }
    public bool IsRequired { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class QuestionOptionModel
{
    public required string QuestionOptionId { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AnswerModel
{
    public required string AnswerId { get; set; }
    public required string QuestionId { get; set; }
    public string? Description { get; set; }
    public int OrderSort { get; set; }
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SurveyScoreOutcomeOutputModel
{
    public string? Id { get; set; }
    public string? SurveyId { get; set; }
    public int MinScore { get; set; }
    public int MaxScore { get; set; }
    public string? ConditionLabel { get; set; }
    public string? ResultTitle { get; set; }
    public string? ResultDescription { get; set; }
    public string? Recommendation { get; set; }
}