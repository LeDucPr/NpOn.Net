using ProtoBuf;

namespace QuestionServiceObject.CommandObjects;

[ProtoContract]
public class SurveyAddOrUpdateCommand : BaseQuestionQuery
{
    [ProtoMember(1)] public string? Id { get; set; } // update thì có Id, không có -> add new
    [ProtoMember(2)] public string? Title { get; set; }
    [ProtoMember(3)] public string? Description { get; set; }
    [ProtoMember(4)] public bool? IsPublished { get; set; }
    [ProtoMember(5)] public DateTime? ExpiredAt { get; set; }
}
