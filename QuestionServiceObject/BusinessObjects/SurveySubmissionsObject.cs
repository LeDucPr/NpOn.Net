using ObjectHandlerFlow.AlgObjs.Attributes;
using ProtoBuf;

namespace QuestionServiceObject.BusinessObjects;

[ProtoContract]
[TableLoader("survey_submissions")]
public class SurveySubmissionsObject : BaseQuestionObject
{
    [ProtoMember(1)] public Guid SurveyId { get; set; }
    [ProtoMember(2)] public Guid? UserId { get; set; }
    [ProtoMember(3)] public Guid SessionToken { get; set; }
    [ProtoMember(4)] public int TotalScore { get; set; }
    [ProtoMember(5)] public Guid? ResultCategoryId { get; set; }
    [ProtoMember(6)] public DateTime StartedAt { get; set; }
    [ProtoMember(7)] public DateTime? SubmittedAt { get; set; }
    [ProtoMember(8)] public int? TimeTakenSeconds { get; set; }
    [ProtoMember(9)] public string? Status { get; set; }
    [ProtoMember(10)] public string? IpAddress { get; set; }
    [ProtoMember(11)] public string? UserAgent { get; set; }
    [ProtoMember(12)] public string? Metadata { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new Dictionary<string, string>();
        FieldMap.Add(nameof(SurveyId), "survey_id");
        FieldMap.Add(nameof(UserId), "user_id");
        FieldMap.Add(nameof(SessionToken), "session_token");
        FieldMap.Add(nameof(TotalScore), "total_score");
        FieldMap.Add(nameof(ResultCategoryId), "result_category_id");
        FieldMap.Add(nameof(StartedAt), "started_at");
        FieldMap.Add(nameof(SubmittedAt), "submitted_at");
        FieldMap.Add(nameof(TimeTakenSeconds), "time_taken_seconds");
        FieldMap.Add(nameof(Status), "status");
        FieldMap.Add(nameof(IpAddress), "ip_address");
        FieldMap.Add(nameof(UserAgent), "user_agent");
        FieldMap.Add(nameof(Metadata), "metadata");
        base.FieldMapper();
    }
}
