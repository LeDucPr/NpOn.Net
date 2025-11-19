using ObjectHandlerFlow.AlgObjs.Attributes;
using ProtoBuf;

namespace QuestionServiceObject.BusinessObjects;

[ProtoContract]
[TableLoader("questions")]
[ProtoInclude(250, typeof(QuestionWithOptionsObject))]
public class QuestionObject : BaseQuestionObject
{
    [ProtoMember(1)] public Guid SurveyId { get; set; }
    [ProtoMember(2)] public string QuestionText { get; set; }
    [ProtoMember(3)] public string QuestionType { get; set; }
    [ProtoMember(4)] public int QuestionOrder { get; set; }
    [ProtoMember(5)] public bool IsRequired { get; set; }
    [ProtoMember(6)] public int MaxScore { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new Dictionary<string, string>();
        FieldMap.Add(nameof(SurveyId), "survey_id");
        FieldMap.Add(nameof(QuestionText), "question_text");
        FieldMap.Add(nameof(QuestionType), "question_type");
        FieldMap.Add(nameof(QuestionOrder), "question_order");
        FieldMap.Add(nameof(IsRequired), "is_required");
        FieldMap.Add(nameof(MaxScore), "max_score");
        base.FieldMapper();
    }
}


[ProtoContract]
public class QuestionWithOptionsObject : QuestionObject
{
    [ProtoMember(7)] public List<AnswerOptionsObject> Options { get; set; }

    public QuestionWithOptionsObject()
    {
        Options = new List<AnswerOptionsObject>();
    }
}
