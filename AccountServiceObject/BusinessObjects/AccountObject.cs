using CommonMode;
using ProjectEnums.AccountEnums;
using ProtoBuf;

namespace AccountServiceObject.BusinessObjects;

[ProtoContract]
public class AccountObject : BaseAccountObjectFromGrpcTable
{
    [ProtoMember(1)] public required Guid Id { get; set; } // AccountId
    [ProtoMember(2)] public required string UserName { get; set; }
    [ProtoMember(3)] public required string Password { get; set; }
    [ProtoMember(4)] public string? FullName { get; set; }
    [ProtoMember(5)] public string? PhoneNumber { get; set; }
    [ProtoMember(6)] public DateTime? CreatedAt { get; set; }
    [ProtoMember(7)] public DateTime? UpdatedAt { get; set; }
    [ProtoMember(8)] public string? AvatarUrl { get; set; }
    [ProtoMember(9)] public EPermission? Permission { get; set; } // Flags
    public EPermission[]? Permissions => Permission?.GetFlags<EPermission>();

    protected override void FieldMapper()
    {
        FieldMap ??= new();
        FieldMap.Add(nameof(Id), "id");
        FieldMap.Add(nameof(UserName), "username");
        FieldMap.Add(nameof(Password), "password");
        FieldMap.Add(nameof(FullName), "full_name");
        FieldMap.Add(nameof(PhoneNumber), "phone_number");
        FieldMap.Add(nameof(CreatedAt), "created_at");
        FieldMap.Add(nameof(UpdatedAt), "updated_at");
        FieldMap.Add(nameof(AvatarUrl), "avatar_url");
        FieldMap.Add(nameof(Permission), "permission");
    }
}