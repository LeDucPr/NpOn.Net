using ProtoBuf;

namespace QuestionServiceObject.CommandObjects;

[ProtoContract]
public class SurveyAddCommand : BaseQuestionQuery
{
    [ProtoMember(1)] public string? Title { get; set; }
    [ProtoMember(1)] public string? Description { get; set; }
    [ProtoMember(1)] public bool? IsPublished { get; set; }
    [ProtoMember(1)] public DateTime? ExpiredAt { get; set; }
}
