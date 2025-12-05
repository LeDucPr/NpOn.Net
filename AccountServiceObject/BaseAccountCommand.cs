using AccountServiceObject.CommandObjects;
using AccountServiceObject.QueryObjects;
using CommonGrpcObject;
using Enums;
using ProtoBuf;

namespace AccountServiceObject;

[ProtoContract]
[ProtoInclude(100, typeof(AccountLoginQuery))]
[ProtoInclude(200, typeof(AccountRefreshTokenQuery))]
[ProtoInclude(300, typeof(AccountLogoutQuery))]
[ProtoInclude(400, typeof(AccountSignupCommand))]
public abstract class BaseAccountCommand : CommonAbsQuery
{
    [ProtoMember(1)] public override bool Status { get; set; }
    [ProtoMember(2)] public override EErrorCode? ErrorCode { get; set; }
    [ProtoMember(3)] public override string? Object { get; set; }
    [ProtoMember(4)] public sealed override DateTime QueryUtcTime { get; init; } = DateTime.UtcNow;
    [ProtoMember(5)] public virtual string? ProcessUId { get; set; }
    // [ProtoMember(6)] public string? LoginUId { get; set; }
}