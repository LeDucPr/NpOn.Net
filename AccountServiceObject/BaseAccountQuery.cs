using AccountServiceObject.QueryObjects;
using CommonGrpcObject;
using Enums;
using ProtoBuf;

namespace AccountServiceObject;

[ProtoContract]
[ProtoInclude(100, typeof(AccountLoginQuery))]
public abstract class BaseAccountQuery : CommonAbsQuery
{
    [ProtoMember(1)] public override bool Status { get; set; }
    [ProtoMember(2)] public override EErrorCode? ErrorCode { get; set; }
    [ProtoMember(3)] public override string? Object { get; set; }
    [ProtoMember(4)] public sealed override DateTime QueryUtcTime { get; init; } = DateTime.UtcNow;
}