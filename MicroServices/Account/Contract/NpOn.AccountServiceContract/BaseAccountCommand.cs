using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonGrpcContract;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract;

[ProtoContract]
[ProtoInclude(100, typeof(AccountLoginQuery))]
[ProtoInclude(200, typeof(AccountRefreshTokenQuery))]
[ProtoInclude(300, typeof(AccountLogoutQuery))]
[ProtoInclude(400, typeof(AccountSignupCommand))]
[ProtoInclude(500, typeof(AccountInfoGetByAccountIdQuery))]
[ProtoInclude(600, typeof(AccountInfoAddOrChangeCommand))]
[ProtoInclude(700, typeof(AccountPermissionExceptionGetByAccountIdQuery))]
[ProtoInclude(800, typeof(AccountPermissionControllerAddOrChangeCommand))]
[ProtoInclude(900, typeof(AccountPermissionControllerDeleteByHostCodeAndVersionIdCommand))]
[ProtoInclude(1000, typeof(AccountPermissionExceptionAddOrChangeCommand))]
[ProtoInclude(1100, typeof(AccountPermissionExceptionCachingCheckValidQuery))]
[ProtoInclude(1200, typeof(AccountSetStatusCommand))]
[ProtoInclude(1300, typeof(AccountChangePasswordCommand))]
[ProtoInclude(1400, typeof(AccountGroupAddOrChangeCommand))]
[ProtoInclude(1500, typeof(AccountGroupDeleteCommand))]
[ProtoInclude(1600, typeof(AccountPermissionExceptionAddOrChangeManyCommand))]
[ProtoInclude(1700, typeof(AccountGroupCopyCommand))]
[ProtoInclude(1800, typeof(AccountGroupSearchQuery))]
[ProtoInclude(1900, typeof(AccountAddressAddOrChangeCommand))]
[ProtoInclude(2000, typeof(AccountSyncFromOldSystemCommand))]
[ProtoInclude(2100, typeof(AccountMenuSearchQuery))]
[ProtoInclude(2200, typeof(AccountMenuGetByIdQuery))]
[ProtoInclude(2300, typeof(AccountInfoGetByAccountIdsQuery))]
[ProtoInclude(2400, typeof(AccountMenuGetByParentIdQuery))]

public abstract class BaseAccountCommand : CommonAbsQuery
{
    [ProtoMember(1)] public override bool Status { get; set; }
    [ProtoMember(2)] public override EErrorCode? ErrorCode { get; set; }
    [ProtoMember(3)] public override string? Object { get; set; }
    [ProtoMember(4)] public sealed override DateTime QueryUtcTime { get; init; } = DateTime.UtcNow;
    [ProtoMember(5)] public virtual string? ProcessUId { get; set; }
    // [ProtoMember(6)] public string? LoginUId { get; set; }
}