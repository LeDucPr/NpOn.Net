using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;

[ProtoContract]
public class AccountSignupCommand : BaseAccountCommand
{
    [ProtoMember(1)] public required string Email { get; set; }
    [ProtoMember(2)] public required string PhoneNumber { get; set; }
    [ProtoMember(3)] public required string UserName { get; set; }
    [ProtoMember(4)] public required string Password { get; set; }
    [ProtoMember(5)] public ELoginType? LoginType { get; set; } = ELoginType.Default;
    [ProtoMember(6)] public required EAuthentication AuthType { get; set; }
    [ProtoMember(7)] public required string ClientId { get; set; }
    [ProtoMember(8)] public string? SignupIp { get; set; }
    [ProtoMember(9)] public string? DeviceSignupInfo { get; set; }
    [ProtoMember(10)] public string? AuthenApplicationId { get; set; }
    [ProtoMember(11)] public required string FullName { get; set; }
    [ProtoMember(12)] public string? AvatarUrl { get; set; }
}

[ProtoContract]
public class AccountSetStatusCommand : BaseAccountCommand
{
    [ProtoMember(1)] public required Guid AccountId{ get; set; }
    [ProtoMember(2)] public EAccountStatus AccountStatus { get; set; }
}

[ProtoContract]
public class AccountChangePasswordCommand : BaseAccountCommand
{
    [ProtoMember(1)] public required Guid AccountId{ get; set; }
    [ProtoMember(3)] public required string NewPassword { get; set; }
}

 
