using ProjectEnums.AccountEnums;
using ProtoBuf;

namespace AccountServiceObject.BusinessObjects;

[ProtoContract]
public class AccountObject
{
    [ProtoMember(1)] public required Guid AccountId { get; set; }
    [ProtoMember(2)] public required string UserName { get; set; }
    [ProtoMember(3)] public required string Password { get; set; }
    [ProtoMember(4)] public required EAuthentication AuthType { get; set; }
    [ProtoMember(5)] public required ELoginType LoginType { get; set; }
    [ProtoMember(6)] public string? FullName { get; set; }
    [ProtoMember(7)] public string? PhoneNumber { get; set; }
    [ProtoMember(8)] public string? DeviceId { get; set; }
    // [ProtoMember(9)] public string? DeviceIdAsString { get; set; }
    [ProtoMember(10)] public string? Token { get; set; }
    [ProtoMember(11)] public string? RefreshToken { get; set; }
    [ProtoMember(12)] public DateTime? CreatedAt { get; set; }
    [ProtoMember(13)] public DateTime? UpdatedAt { get; set; }
    [ProtoMember(14)] public string? AvatarUrl { get; set; }
    [ProtoMember(15)] public DateTime? InitDate { get; set; }
    [ProtoMember(16)] public int? MinuteExpire { get; set; }
    [ProtoMember(17)] public EPermission? Permissions { get; set; } // Flags
}