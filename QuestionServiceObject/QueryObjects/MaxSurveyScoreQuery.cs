using ProtoBuf;

namespace QuestionServiceObject.QueryObjects;

[ProtoContract]
public class MaxSurveyScoreQuery
{
    [ProtoMember(1)] public required string SurveyId { get; set; }
}