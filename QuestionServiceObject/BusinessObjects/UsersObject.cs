using ObjectHandlerFlow.AlgObjs.Attributes;
using ProtoBuf;

namespace QuestionServiceObject.BusinessObjects;

[ProtoContract]
[TableLoader("users")]
public class UsersObject : BaseQuestionObject
{
    [ProtoMember(1)] public string Email { get; set; }
    [ProtoMember(2)] public string FullName { get; set; }
    [ProtoMember(3)] public string Phone { get; set; }
    [ProtoMember(4)] public DateTime? FirstSubmissionAt { get; set; }
    [ProtoMember(5)] public DateTime? LastSubmissionAt { get; set; }
    [ProtoMember(6)] public int TotalSubmissions { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new Dictionary<string, string>();
        FieldMap.Add(nameof(Email), "email");
        FieldMap.Add(nameof(FullName), "full_name");
        FieldMap.Add(nameof(Phone), "phone");
        FieldMap.Add(nameof(FirstSubmissionAt), "first_submission_at");
        FieldMap.Add(nameof(LastSubmissionAt), "last_submission_at");
        FieldMap.Add(nameof(TotalSubmissions), "total_submissions");
        base.FieldMapper();
    }
}
