using ProjectEnums.AccountEnums;
using ProtoBuf;

namespace AccountServiceObject.QueryObjects;

[ProtoContract]
public class AccountLoginQuery : BaseAccountQuery
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
}

[ProtoContract]
public class AccountRefreshTokenQuery : BaseAccountQuery
{
    [ProtoMember(1)] public required string RefreshToken { get; set; }
    [ProtoMember(2)] public string? DeviceInfo { get; set; }
    [ProtoMember(3)] public ELoginType? LoginType { get; set; }
}