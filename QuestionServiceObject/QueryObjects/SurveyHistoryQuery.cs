using ProtoBuf;

namespace QuestionServiceObject.QueryObjects;

[ProtoContract]
public class SurveyHistoryQuery
{
    [ProtoMember(1)] public string? ResultId { get; set; }

    [ProtoMember(2)] public string? UserId { get; set; }

    [ProtoMember(3)] public string? SurveyId { get; set; }

    [ProtoMember(4)] public int PageIndex { get; set; }

    [ProtoMember(5)] public int PageSize { get; set; }
}