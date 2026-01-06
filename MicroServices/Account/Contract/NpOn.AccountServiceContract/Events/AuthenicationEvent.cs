using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Events;

[ProtoContract]
[ProtoInclude(100, typeof(AccountSaveLogoutEvent))]
public class AccountSaveLoginEvent : BaseAccountCommonEvent
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
    [ProtoMember(13)] public DateTime? CreatedAt { get; set; }
    [ProtoMember(14)] public DateTime? UpdatedAt { get; set; }
    [ProtoMember(15)] public required string SessionId { get; set; }
    [ProtoMember(16)] public int MinuteExpire { get; set; }
    [ProtoMember(17)] public ETokenStatus TokenStatus { get; set; } = ETokenStatus.Inactive;
    [ProtoMember(18)] public string? Email { get; set; }
    [ProtoMember(19)] public string? AvatarUrl { get; set; }
}

[ProtoContract]
public class AccountSaveLogoutEvent : AccountSaveLoginEvent
{
}

public static class AccountSaveLoginEventExtensions
{
    public static AccountLoginRModel ToObject(this AccountSaveLoginEvent @event)
    {
        return new AccountLoginRModel()
        {
            Id = @event.Id,
            AccountId = @event.AccountId,
            UserName = @event.UserName,
            Password = @event.Password,
            AuthType = @event.AuthType,
            LoginType = @event.LoginType,
            Permission = @event.Permission,
            FullName = @event.FullName,
            PhoneNumber = @event.PhoneNumber,
            DeviceId = @event.DeviceId,
            Token = @event.Token,
            RefreshToken = @event.RefreshToken,
            CreatedAt = @event.CreatedAt,
            UpdatedAt = @event.UpdatedAt,
            SessionId = @event.SessionId,
            MinuteExpire = @event.MinuteExpire,
            TokenStatus = @event.TokenStatus,
            Email = @event.Email,
            AvatarUrl = @event.AvatarUrl,
        };
    }
    
    public static AccountLoginRModel ToObject(this AccountSaveLogoutEvent @event)
    {
        return new AccountLoginRModel()
        {
            Id = @event.Id,
            AccountId = @event.AccountId,
            UserName = @event.UserName,
            Password = @event.Password,
            AuthType = @event.AuthType,
            LoginType = @event.LoginType,
            Permission = @event.Permission,
            FullName = @event.FullName,
            PhoneNumber = @event.PhoneNumber,
            DeviceId = @event.DeviceId,
            Token = @event.Token,
            RefreshToken = @event.RefreshToken,
            CreatedAt = @event.CreatedAt,
            UpdatedAt = @event.UpdatedAt,
            SessionId = @event.SessionId,
            MinuteExpire = @event.MinuteExpire,
            TokenStatus = @event.TokenStatus,
            Email = @event.Email,
            AvatarUrl = @event.AvatarUrl,
        };
    }
}