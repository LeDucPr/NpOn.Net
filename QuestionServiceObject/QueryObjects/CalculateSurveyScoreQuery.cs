using ProtoBuf;

namespace QuestionServiceObject.QueryObjects;

[ProtoContract]
public class CalculateSurveyScoreQuery : BaseQuestionQuery
{
    [ProtoMember(1)] public required string SurveyId { get; set; }
    [ProtoMember(2)] public List<SubmitAnswerQuery> Answers { get; set; } = [];
}

[ProtoContract]
public class SubmitAnswerQuery
{
    [ProtoMember(1)] public required string QuestionId { get; set; }
    [ProtoMember(2)] public List<string> SelectedOptionIds { get; set; } = [];
}
