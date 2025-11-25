using ObjectHandlerFlow.AlgObjs.Attributes;
using ProtoBuf;

namespace QuestionServiceObject.BusinessObjects;

[ProtoContract]
[TableLoader("ques_srv_survey_outcome")]
public class SurveyOutcomeObject : BaseQuestionObject
{
    [ProtoMember(1)] public Guid QuesSrvSurveyId { get; set; }
    [ProtoMember(2)] public int MinScore { get; set; }
    [ProtoMember(3)] public int? MaxScore { get; set; }
    [ProtoMember(4)] public string? ConditionLabel { get; set; }
    [ProtoMember(5)] public string? ResultTitle { get; set; }
    [ProtoMember(6)] public string? ResultDescription { get; set; }
    [ProtoMember(7)] public string? Recommendation { get; set; }
    [ProtoMember(8)] public bool IsActive { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new Dictionary<string, string>();
        FieldMap.Add(nameof(QuesSrvSurveyId), "ques_srv_survey_id");
        FieldMap.Add(nameof(MinScore), "min_score");
        FieldMap.Add(nameof(MaxScore), "max_score");
        FieldMap.Add(nameof(ConditionLabel), "condition_label");
        FieldMap.Add(nameof(ResultTitle), "result_title");
        FieldMap.Add(nameof(ResultDescription), "result_description");
        FieldMap.Add(nameof(Recommendation), "recommendation");
        FieldMap.Add(nameof(IsActive), "is_active");
        base.FieldMapper();
    }
}