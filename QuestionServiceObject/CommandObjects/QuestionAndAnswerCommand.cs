using ProtoBuf;

namespace QuestionServiceObject.CommandObjects;

[ProtoContract]
public class SubmissionAnswer
{
    [ProtoMember(1)] public string? QuestionId { get; set; }
    [ProtoMember(2)] public List<string>? AnswerIds { get; set; }
    [ProtoMember(3)] public string? TextAnswer { get; set; }
}

[ProtoContract]
public class SubmitSurveyCommand : BaseQuestionQuery
{
    [ProtoMember(1)] public required string UserId { get; set; }
    [ProtoMember(2)] public required string SurveyId { get; set; }
    [ProtoMember(3)] public List<SubmissionAnswer> Answers { get; set; } = new();
}