using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

[ProtoContract]
public sealed class AccountGroupRModel : BaseAccountRModelFromGrpcTable
{
    [ProtoMember(1)] public Guid GroupId { get; set; }
    [ProtoMember(2)] public Guid Leader { get; set; }
    [ProtoMember(3)] public Guid? Member { get; set; } // first record when create maybe null
    [ProtoMember(4)] public string? GroupName { get; set; }
    [ProtoMember(5)] public EAccountGroupType? GroupType { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(GroupId), "id");
        FieldMap.Add(nameof(Leader), "acc_srv_account_leader_id");
        FieldMap.Add(nameof(Member), "acc_srv_account_member");
        FieldMap.Add(nameof(GroupType), "group_type");
        FieldMap.Add(nameof(GroupName), "group_name");
    }
}