using ProjectEnums.AccountEnums;
using ProtoBuf;

namespace AccountServiceObject.CommandObjects;

[ProtoContract]
public class AccountSigninCommand : BaseAccountCommand
{
    [ProtoMember(1)] public required string Email { get; set; }
    [ProtoMember(2)] public required string PhoneNumber { get; set; }
    [ProtoMember(3)] public required string UserName { get; set; }
    [ProtoMember(4)] public required string Password { get; set; }
    [ProtoMember(5)] public ELoginType? LoginType { get; set; } = ELoginType.Default;
    [ProtoMember(6)] public required EAuthentication AuthType { get; set; }
    [ProtoMember(7)] public required string ClientId { get; set; }
    [ProtoMember(8)] public string? SigninIp { get; set; }
    [ProtoMember(9)] public string? DeviceSigninInfo { get; set; }
    [ProtoMember(10)] public string? AuthenApplicationId { get; set; }
    [ProtoMember(11)] public required string FullName { get; set; }
    [ProtoMember(12)] public string? AvatarUrl { get; set; }
}