using ProtoBuf;

namespace QuestionServiceObject.BusinessObjects;

[ProtoContract]
public class FaqObject : BaseQuestionObject
{
    [ProtoMember(1)] public string CCCC { get; set; }
}