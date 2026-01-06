using Common.Extensions.NpOn.CommonBaseDomain.Attributes;
using Common.Extensions.NpOn.HandleFlow.Attributes;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;

[ProtoContract]
[TableLoader("acc_srv_account_permission_exception")]
public sealed class AccountPermissionException : BaseAccountDomain
{
    [ProtoMember(1)]
    [Field("acc_srv_account_id")]
    [Pk("acc_srv_account_id")]
    public Guid AccountId { get; set; }

    [ProtoMember(2)]
    [Field("acc_srv_account_permission_controller_code")]
    [Pk("acc_srv_account_permission_controller_code")]
    public string ControllerCode { get; set; } = string.Empty;
    
    [ProtoMember(3)]
    [Field("access_permission")]
    public EPermissionAccessController AccessPermission { get; set; }
    
    [ProtoMember(4)] [Field("created_at")] public DateTime CreatedAt { get; set; }

    [ProtoMember(5)] [Field("updated_at")] public DateTime? UpdatedAt { get; set; }

    public static AccountPermissionException FromCommands(AccountPermissionExceptionAddOrChangeCommand command)
    {
        return new AccountPermissionException()
        {
            AccountId = command.AccountId,
            ControllerCode = command.ControllerCode,
            AccessPermission = command.AccessPermission,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };
    }
}