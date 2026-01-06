using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;

[ProtoContract]
public class AccountLoginQuery : BaseAccountCommand
{
    [ProtoMember(1)] public string? Email { get; set; }
    [ProtoMember(2)] public string? PhoneNumber { get; set; }
    [ProtoMember(3)] public string? UserName { get; set; }
    [ProtoMember(4)] public string? Password { get; set; }
    [ProtoMember(5)] public ELoginType? LoginType { get; set; } = ELoginType.Default;
    [ProtoMember(6)] public required EAuthentication AuthType { get; set; }
    [ProtoMember(7)] public required string ClientId { get; set; }
    [ProtoMember(8)] public string? Ip { get; set; }
    [ProtoMember(9)] public string? DeviceLoginInfo { get; set; }

    [ProtoMember(10)] public string? AuthenApplicationId { get; set; }

    // [ProtoMember(11)] public string? ExternalLoginId { get; set; }
    [ProtoMember(12)] public bool IsEnableMultiDevice { get; set; } = false;
}

[ProtoContract]
public class AccountRefreshTokenQuery : BaseAccountCommand
{
    [ProtoMember(1)] public required string RefreshToken { get; set; }
    [ProtoMember(2)] public string? DeviceInfo { get; set; }
    [ProtoMember(3)] public ELoginType? LoginType { get; set; }
    [ProtoMember(4)] public required EAuthentication AuthType { get; set; }
    [ProtoMember(5)] public required string SessionId { get; set; }
}

[ProtoContract]
[ProtoInclude(100, typeof(AccountGetLogonInfoBySessionIdQuery))]
public class AccountLogoutQuery : BaseAccountCommand
{
    [ProtoMember(1)] public required string SessionId { get; set; }
}

[ProtoContract]
public class AccountGetLogonInfoBySessionIdQuery : AccountLogoutQuery
{
}


[ProtoContract]
public class AccountPermissionExceptionCachingCheckValidQuery : BaseAccountCommand
{
    [ProtoMember(1)] public required string AccountId { get; set; }
    [ProtoMember(2)] public required string ControllerCode { get; set; }
    [ProtoMember(3)] public required bool IsHasBasePermission { get; set; }
}
