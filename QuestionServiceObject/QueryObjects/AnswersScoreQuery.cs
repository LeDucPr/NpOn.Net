using ProtoBuf;

namespace QuestionServiceObject.QueryObjects;

[ProtoContract]
public class AnswersScoreQuery
{
    [ProtoMember(1)] public required string AnswerIds { get; set; }
}