using Common.Extensions.NpOn.CommonBaseDomain.Attributes;
using Common.Extensions.NpOn.HandleFlow.Attributes;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;

[ProtoContract]
[TableLoader("acc_srv_account_login")]
public sealed class AccountLogin : BaseAccountDomain
{
    [ProtoMember(1)]
    [Field("id")]
    [Pk("id")]
    public Guid? Id { get; set; }

    [ProtoMember(2)]
    [Field("acc_srv_account_id")]
    public Guid AccountId { get; set; }

    [ProtoMember(3)] [Field("username")] public string UserName { get; set; }
    [ProtoMember(4)] [Field("password")] public string Password { get; set; }
    [ProtoMember(5)] [Field("auth_type")] public EAuthentication AuthType { get; set; }
    [ProtoMember(6)] [Field("login_type")] public ELoginType LoginType { get; set; }
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
    public string SessionId { get; set; }

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

    public AccountLogin()
    {
    }

    public AccountLogin(AccountLoginRModel loginRModel)
    {
        Id = loginRModel.Id;
        AccountId = loginRModel.AccountId;
        UserName = loginRModel.UserName;
        Password = loginRModel.Password;
        AuthType = loginRModel.AuthType;
        LoginType = loginRModel.LoginType;
        Permission = loginRModel.Permission;
        FullName = loginRModel.FullName;
        PhoneNumber = loginRModel.PhoneNumber;
        DeviceId = loginRModel.DeviceId;
        Token = loginRModel.Token;
        RefreshToken = loginRModel.RefreshToken;
        CreatedAt = loginRModel.CreatedAt;
        UpdatedAt = loginRModel.UpdatedAt;
        SessionId = loginRModel.SessionId;
        MinuteExpire = loginRModel.MinuteExpire;
        TokenStatus = loginRModel.TokenStatus;
        Email = loginRModel.Email;
        AvatarUrl = loginRModel.AvatarUrl;
    }

    public void Logout()
    {
        TokenStatus = ETokenStatus.Inactive;
    }
}