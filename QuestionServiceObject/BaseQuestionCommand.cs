using CommonGrpcObject;
using Enums;
using ProtoBuf;
using QuestionServiceObject.CommandObjects;

namespace QuestionServiceObject;

[ProtoContract]
[ProtoInclude(100, typeof(SurveyAddOrUpdateCommand))]
[ProtoInclude(200, typeof(SubmitSurveyCommand))]
public abstract class BaseQuestionCommand : CommonAbsQuery
{
    [ProtoMember(1)] public override bool Status { get; set; }
    [ProtoMember(2)] public override EErrorCode? ErrorCode { get; set; }
    [ProtoMember(3)] public override string? Object { get; set; }
    [ProtoMember(4)] public sealed override DateTime QueryUtcTime { get; init; } = DateTime.UtcNow;
}
