using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

[ProtoContract]
public class AccountPermissionExceptionRModel : BaseAccountRModelFromGrpcTable
{
    [ProtoMember(1)] public Guid AccountId { get; set; }
    [ProtoMember(2)] public string ControllerCode { get; set; } = string.Empty;
    [ProtoMember(3)] public EPermissionAccessController AccessPermission { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(AccountId), "acc_srv_account_id");
        FieldMap.Add(nameof(ControllerCode), "acc_srv_account_permission_controller_code");
        FieldMap.Add(nameof(AccessPermission), "access_permission");
    }
}