using ProtoBuf;

namespace QuestionServiceObject.CommandObjects;

[ProtoContract]
public class SubmitSurveyCommand : BaseQuestionCommand
{
    [ProtoMember(1)] public required string SurveyId { get; set; }
    [ProtoMember(2)] public string? UserId { get; set; }
    [ProtoMember(3)] public List<SubmitAnswerCommand> Answers { get; set; } = [];
    [ProtoMember(4)] public int TotalScore { get; set; }
    [ProtoMember(5)] public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

[ProtoContract]
public class SubmitAnswerCommand
{
    [ProtoMember(1)] public required string QuestionId { get; set; }
    [ProtoMember(2)] public string? TextAnswer { get; set; }
    [ProtoMember(3)] public List<string> SelectedOptionIds { get; set; } = [];
    [ProtoMember(4)] public int ScoreEarned { get; set; }
}
