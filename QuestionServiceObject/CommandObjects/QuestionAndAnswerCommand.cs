using ProtoBuf;

namespace QuestionServiceObject.CommandObjects;

[ProtoContract]
public class SubmissionAnswer
{
    [ProtoMember(1)] public string? QuestionId { get; set; } // Guid
    [ProtoMember(2)] public List<string>? AnswerIds { get; set; } // List of Guid
    [ProtoMember(3)] public string? TextAnswer { get; set; } // null if existed AnswerIds
}

[ProtoContract]
public class SubmitSurveyCommand : BaseQuestionQuery
{
    [ProtoMember(1)] public required string UserId { get; set; } // Guid
    [ProtoMember(2)] public required string SurveyId { get; set; } // Guid
    [ProtoMember(3)] public List<SubmissionAnswer> Answers { get; set; } = new();
}