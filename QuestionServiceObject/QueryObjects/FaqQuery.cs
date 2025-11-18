using ProtoBuf;

namespace QuestionServiceObject.QueryObjects;

[ProtoContract]
public class FaqQuery : BaseQuestionQuery
{
    [ProtoMember(1)] public string? Email { get; set; }
}