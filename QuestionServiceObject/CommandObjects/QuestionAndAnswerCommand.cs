using ProtoBuf;

namespace QuestionServiceObject.CommandObjects;

[ProtoContract]
public class QuestionAndAnswerAddOrUpdateCommand : BaseQuestionQuery
{
    [ProtoMember(1)] public string? Question { get; set; }
}