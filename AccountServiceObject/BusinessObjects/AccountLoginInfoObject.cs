using ProjectEnums.AccountEnums;
using ProtoBuf;

namespace AccountServiceObject.BusinessObjects;

[ProtoContract]
public class AccountLoginInfoObject : BaseAccountObject
{
    [ProtoMember(1)] public required Guid AccountId { get; set; }
    [ProtoMember(2)] public required string UserName { get; set; }
    [ProtoMember(3)] public required string Password { get; set; }
    [ProtoMember(4)] public required EAuthentication AuthType { get; set; }
    [ProtoMember(5)] public required ELoginType LoginType { get; set; }
    [ProtoMember(6)] public string? FullName { get; set; }
    [ProtoMember(7)] public string? PhoneNumber { get; set; }
    [ProtoMember(8)] public Guid? DeviceId { get; set; }
    // [ProtoMember(9)] public string? DeviceIdAsString { get; set; }
    [ProtoMember(10)] public string? Token { get; set; }
    [ProtoMember(11)] public string? RefreshToken { get; set; }
    [ProtoMember(12)] public DateTime? CreatedAt { get; set; }
    [ProtoMember(13)] public DateTime? UpdatedAt { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new();
        FieldMap.Add(nameof(AccountId), "account_id");
        FieldMap.Add(nameof(UserName), "username");
        FieldMap.Add(nameof(Password), "password");
        FieldMap.Add(nameof(AuthType), "auth_type");
        FieldMap.Add(nameof(LoginType), "login_type");
        FieldMap.Add(nameof(FullName), "full_name");
        FieldMap.Add(nameof(PhoneNumber), "phone_number");
        FieldMap.Add(nameof(DeviceId), "device_id");
        FieldMap.Add(nameof(Token), "token");
        FieldMap.Add(nameof(RefreshToken), "refresh_token");
        FieldMap.Add(nameof(CreatedAt), "created_at");
        FieldMap.Add(nameof(UpdatedAt), "updated_at");
        base.FieldMapper();
    }
}