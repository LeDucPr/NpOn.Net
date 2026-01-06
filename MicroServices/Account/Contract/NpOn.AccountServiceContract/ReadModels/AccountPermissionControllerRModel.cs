using Common.Extensions.NpOn.CommonMode;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

[ProtoContract]
public class AccountPermissionControllerRModel : BaseAccountRModelFromGrpcTable
{
    [ProtoMember(1)] public required string HostCode { get; set; }
    [ProtoMember(2)] public required Guid VersionId { get; set; }
    [ProtoMember(3)] public required string Code { get; set; }
    [ProtoMember(4)] public EPermission Permission { get; set; }
    [ProtoMember(5)] public string Description { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new Dictionary<string, string>();
        FieldMap.Add(nameof(HostCode), "host_code");
        FieldMap.Add(nameof(VersionId), "version_id");
        FieldMap.Add(nameof(Code), "code");
        FieldMap.Add(nameof(Permission), "permission");
        FieldMap.Add(nameof(Description), "description");
    }

    public EPermission[] GetEnablePermissions => Permission.GetFlags(); 
}