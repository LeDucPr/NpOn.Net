using ObjectHandlerFlow.AlgObjs.Attributes;
using ProjectBaseDomain;
using ProjectBaseDomain.Attributes;
using ProjectEnums.AccountEnums;
using ProtoBuf;

namespace AccountServiceObject.Domains;

[TableLoader("acc_srv_account_login")]
public sealed class AccountLoginInfoDomain : BaseDomain
{
    [ProtoMember(1)]
    [Field("id")]
    [Pk("id")]
    public Guid? Id { get; set; }

    [ProtoMember(2)]
    [Field("acc_srv_account_id")]
    public required Guid AccountId { get; set; }

    [ProtoMember(3)] [Field("username")] public required string UserName { get; set; }
    [ProtoMember(4)] [Field("password")] public required string Password { get; set; }
    [ProtoMember(5)] [Field("auth_type")] public required EAuthentication AuthType { get; set; }
    [ProtoMember(6)] [Field("login_type")] public required ELoginType LoginType { get; set; }
    [ProtoMember(7)] [Field("permission")] public EPermission? Permission { get; set; }
    [ProtoMember(8)] [Field("full_name")] public string? FullName { get; set; }

    [ProtoMember(9)]
    [Field("phone_number")]
    public string? PhoneNumber { get; set; }

    [ProtoMember(10)] [Field("device_id")] public string? DeviceId { get; set; }
    [ProtoMember(11)] [Field("token")] public string? Token { get; set; }

    [ProtoMember(12)]
    [Field("refresh_token")]
    public string? RefreshToken { get; set; }

    [ProtoMember(13)]
    [Field("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ProtoMember(14)]
    [Field("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ProtoMember(15)]
    [Field("session_id")]
    public required string SessionId { get; set; }

    [ProtoMember(16)]
    [Field("minute_expire")]
    public int MinuteExpire { get; set; }

    [ProtoMember(17)]
    [Field("token_status")]
    public ETokenStatus TokenStatus { get; set; } = ETokenStatus.Inactive;

    [ProtoMember(18)] [Field("email")] public string? Email { get; set; }

    [ProtoMember(19)]
    [Field("avatar_url")]
    public string? AvatarUrl { get; set; }
    // [ProtoMember(18)] public string? ReturnUrl { get; set; }
}