using Common.Extensions.NpOn.CommonBaseDomain.Attributes;
using Common.Extensions.NpOn.HandleFlow.Attributes;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;

[ProtoContract]
[TableLoader("acc_srv_account_permission_controller")]
public sealed class AccountPermissionController : BaseAccountDomain
{
    [ProtoMember(1)]
    [Field("host_code")]
    [Pk("host_code")]
    public string HostCode { get; set; }

    [ProtoMember(2)]
    [Field("version_id")]
    public Guid VersionId { get; set; }

    [ProtoMember(3)]
    [Field("code")]
    [Pk("code")]
    public string Code { get; set; }
    
    [ProtoMember(4)]
    [Field("description")]
    public string Description { get; set; }

    [ProtoMember(5)] [Field("permission")] public EPermission Permission { get; set; }
    [ProtoMember(6)] [Field("created_at")] public DateTime CreatedAt { get; set; }
    [ProtoMember(7)] [Field("updated_at")] public DateTime UpdatedAt { get; set; }

    public AccountPermissionController()
    {
    }

    public AccountPermissionController(AccountPermissionControllerAddOrChangeCommand command)
    {
        HostCode = command.HostCode;
        VersionId = command.VersionId;
        Code = command.Code;
        Permission = command.Permission;
        Description = command.Description;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }
}