using Common.Extensions.NpOn.CommonMode;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

[ProtoContract]
public class AccountRModel : BaseAccountRModelFromGrpcTable
{
    [ProtoMember(1)] public required Guid Id { get; set; } // AccountId
    [ProtoMember(2)] public required string UserName { get; set; }
    [ProtoMember(3)] public required string Password { get; set; }
    [ProtoMember(4)] public string? FullName { get; set; }
    [ProtoMember(5)] public string? PhoneNumber { get; set; }
    [ProtoMember(8)] public string? AvatarUrl { get; set; }
    [ProtoMember(9)] public EPermission? Permission { get; set; } // Flags
    public EPermission[]? Permissions => Permission?.GetFlags<EPermission>();
    [ProtoMember(10)] public string? Email { get; set; }
    [ProtoMember(11)] public EAccountStatus Status { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new();
        FieldMap.Add(nameof(Id), "id");
        FieldMap.Add(nameof(UserName), "username");
        FieldMap.Add(nameof(Password), "password");
        FieldMap.Add(nameof(FullName), "full_name");
        FieldMap.Add(nameof(Email), "email");
        FieldMap.Add(nameof(PhoneNumber), "phone_number");
        FieldMap.Add(nameof(AvatarUrl), "avatar_url");
        FieldMap.Add(nameof(Permission), "permission");
        FieldMap.Add(nameof(Status), "status");
    }
}