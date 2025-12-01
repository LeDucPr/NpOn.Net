namespace QuestionServiceObject.CommandObjects;

public class UserAnswerSubmitCommand
{
    public string UserId { get; set; }
    public string QuestionId { get; set; }
    public string[] AnswerIds { get; set; }
    public string? TextAnswer { get; set; }
    public int? ScoreTextAnswer { get; set; }
    public string ResultId { get; set; }
}