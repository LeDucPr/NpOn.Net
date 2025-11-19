using ObjectHandlerFlow.AlgObjs.Attributes;
using ProtoBuf;

namespace QuestionServiceObject.BusinessObjects;

[ProtoContract]
[TableLoader("submission_answers")]
public class SubmissionAnswersObject : BaseQuestionObject
{
    [ProtoMember(1)] public Guid SubmissionId { get; set; }
    [ProtoMember(2)] public Guid QuestionId { get; set; }
    [ProtoMember(3)] public Guid[] SelectedOptionIds { get; set; }
    [ProtoMember(4)] public string TextAnswer { get; set; }
    [ProtoMember(5)] public int ScoreEarned { get; set; }
    [ProtoMember(6)] public DateTime AnsweredAt { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new Dictionary<string, string>();
        FieldMap.Add(nameof(SubmissionId), "submission_id");
        FieldMap.Add(nameof(QuestionId), "question_id");
        FieldMap.Add(nameof(SelectedOptionIds), "selected_option_ids");
        FieldMap.Add(nameof(TextAnswer), "text_answer");
        FieldMap.Add(nameof(ScoreEarned), "score_earned");
        FieldMap.Add(nameof(AnsweredAt), "answered_at");
        base.FieldMapper();
    }
}
