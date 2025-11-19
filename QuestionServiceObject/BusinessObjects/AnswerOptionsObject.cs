using ObjectHandlerFlow.AlgObjs.Attributes;
using ProtoBuf;

namespace QuestionServiceObject.BusinessObjects;

[ProtoContract]
[TableLoader("answer_options")]
public class AnswerOptionsObject : BaseQuestionObject
{
    [ProtoMember(1)] public Guid QuestionId { get; set; }
    [ProtoMember(2)] public string OptionText { get; set; }
    [ProtoMember(3)] public int OptionOrder { get; set; }
    [ProtoMember(4)] public int ScoreValue { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new Dictionary<string, string>();
        FieldMap.Add(nameof(QuestionId), "question_id");
        FieldMap.Add(nameof(OptionText), "option_text");
        FieldMap.Add(nameof(OptionOrder), "option_order");
        FieldMap.Add(nameof(ScoreValue), "score_value");
        base.FieldMapper();
    }
}
