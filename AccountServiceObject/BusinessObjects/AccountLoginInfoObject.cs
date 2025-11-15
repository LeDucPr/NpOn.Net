using Enums.BusinessEnums;
using ProtoBuf;

namespace AccountServiceObject.BusinessObjects;

[ProtoContract]
public class AccountLoginInfoObject : BaseAccountObject
{
    [ProtoMember(1)] public required string UserName { get; set; }
    [ProtoMember(2)] public required string Password { get; set; }
    [ProtoMember(3)] public required EAuthentication AuthType { get; set; }
    [ProtoMember(4)] public string? FullName { get; set; }
    [ProtoMember(5)] public string? PhoneNumber { get; set; }
    [ProtoMember(6)] public bool? IsRacingBoy { get; set; }
    protected override void FieldMapper()
    {
        FieldMap ??= new();
        FieldMap.Add(nameof(UserName), "username");
        FieldMap.Add(nameof(Password), "password");
        FieldMap.Add(nameof(AuthType), "auth_type");
        FieldMap.Add(nameof(FullName), "fullname");
        FieldMap.Add(nameof(PhoneNumber), "phone_number");
        FieldMap.Add(nameof(IsRacingBoy), "is_racing_boy");
        base.FieldMapper();
    }
}