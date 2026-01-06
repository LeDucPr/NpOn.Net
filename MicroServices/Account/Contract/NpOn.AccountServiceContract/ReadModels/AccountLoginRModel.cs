using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Events;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

[ProtoContract]
public sealed class AccountLoginRModel : BaseAccountRModelFromGrpcTable
{
    [ProtoMember(1)] public Guid? Id { get; set; }
    [ProtoMember(2)] public required Guid AccountId { get; set; }
    [ProtoMember(3)] public required string UserName { get; set; }
    [ProtoMember(4)] public required string Password { get; set; }
    [ProtoMember(5)] public required EAuthentication AuthType { get; set; }
    [ProtoMember(6)] public required ELoginType LoginType { get; set; }
    [ProtoMember(7)] public EPermission? Permission { get; set; }
    [ProtoMember(8)] public string? FullName { get; set; }
    [ProtoMember(9)] public string? PhoneNumber { get; set; }
    [ProtoMember(10)] public string? DeviceId { get; set; }
    [ProtoMember(11)] public string? Token { get; set; }
    [ProtoMember(12)] public string? RefreshToken { get; set; }
    [ProtoMember(15)] public required string SessionId { get; set; }
    [ProtoMember(16)] public int MinuteExpire { get; set; }
    [ProtoMember(17)] public ETokenStatus TokenStatus { get; set; } = ETokenStatus.Inactive;
    [ProtoMember(18)] public string? Email { get; set; }
    [ProtoMember(19)] public string? AvatarUrl { get; set; }
    // [ProtoMember(18)] public string? ReturnUrl { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new();
        FieldMap.Add(nameof(Id), "id");
        FieldMap.Add(nameof(AccountId), "acc_srv_account_id");
        FieldMap.Add(nameof(UserName), "username");
        FieldMap.Add(nameof(Password), "password");
        FieldMap.Add(nameof(FullName), "full_name");
        FieldMap.Add(nameof(PhoneNumber), "phone_number");
        FieldMap.Add(nameof(Email), "email");

        FieldMap.Add(nameof(AuthType), "auth_type");
        FieldMap.Add(nameof(LoginType), "login_type");
        FieldMap.Add(nameof(TokenStatus), "token_status");
        FieldMap.Add(nameof(Permission), "permission");
        FieldMap.Add(nameof(AvatarUrl), "avatar_url");

        FieldMap.Add(nameof(DeviceId), "device_id");
        FieldMap.Add(nameof(Token), "token");
        FieldMap.Add(nameof(RefreshToken), "refresh_token");
        FieldMap.Add(nameof(SessionId), "session_id");
        FieldMap.Add(nameof(MinuteExpire), "minute_expire");
    }
}

public static class AccountLoginInfoObjectExtensions
{
    public static AccountSaveLoginEvent ToLoginEvent(this AccountLoginRModel loginRModel, string? oldSessionId = null)
    {
        return new AccountSaveLoginEvent()
        {
            Id = loginRModel.Id,
            AccountId = loginRModel.AccountId,
            UserName = loginRModel.UserName,
            Password = loginRModel.Password,
            AuthType = loginRModel.AuthType,
            LoginType = loginRModel.LoginType,
            Permission = loginRModel.Permission,
            FullName = loginRModel.FullName,
            PhoneNumber = loginRModel.PhoneNumber,
            DeviceId = loginRModel.DeviceId,
            Token = loginRModel.Token,
            RefreshToken = loginRModel.RefreshToken,
            CreatedAt = loginRModel.CreatedAt,
            UpdatedAt = loginRModel.UpdatedAt,
            SessionId = loginRModel.SessionId,
            MinuteExpire = loginRModel.MinuteExpire,
            TokenStatus = loginRModel.TokenStatus,
            Email = loginRModel.Email,
            AvatarUrl = loginRModel.AvatarUrl,
        };
    }

    public static AccountSaveLogoutEvent ToLogoutEvent(this AccountLoginRModel loginRModel, string? oldSessionId = null)
    {
        return new AccountSaveLogoutEvent()
        {
            Id = loginRModel.Id,
            AccountId = loginRModel.AccountId,
            UserName = loginRModel.UserName,
            Password = loginRModel.Password,
            AuthType = loginRModel.AuthType,
            LoginType = loginRModel.LoginType,
            Permission = loginRModel.Permission,
            FullName = loginRModel.FullName,
            PhoneNumber = loginRModel.PhoneNumber,
            DeviceId = loginRModel.DeviceId,
            Token = loginRModel.Token,
            RefreshToken = loginRModel.RefreshToken,
            CreatedAt = loginRModel.CreatedAt,
            UpdatedAt = loginRModel.UpdatedAt,
            SessionId = loginRModel.SessionId,
            MinuteExpire = loginRModel.MinuteExpire,
            TokenStatus = loginRModel.TokenStatus,
            Email = loginRModel.Email,
            AvatarUrl = loginRModel.AvatarUrl,
        };
    }
}