using Common.Infrastructures.NpOn.CommonDb.DbResults.Grpc;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract;

[ProtoContract]
[ProtoInclude(100, typeof(AccountRModel))]
[ProtoInclude(200, typeof(AccountLoginRModel))]
[ProtoInclude(300, typeof(AccountInfoRModel))]
[ProtoInclude(400, typeof(AccountPermissionExceptionRModel))]
[ProtoInclude(500, typeof(AccountPermissionControllerRModel))]
[ProtoInclude(600, typeof(AccountGroupRModel))]
[ProtoInclude(700, typeof(AccountAddressRModel))]
[ProtoInclude(800, typeof(AccountMenuRModel))]
public abstract class BaseAccountRModelFromGrpcTable : NpOnBaseGrpcObject
{
    [ProtoMember(1)] public DateTime? CreatedAt { get; set; }
    [ProtoMember(2)] public DateTime? UpdatedAt { get; set; }
    [ProtoMember(3)] public Guid? ProcessUId { get; set; }
    [ProtoMember(4)] public int? TotalRow { get; set; }

    #region Field Config
    public override Dictionary<string, string>? FieldMap { get; protected set; }

    protected override void BaseFieldMapper()
    {
        base.BaseFieldMapper();
        FieldMap!.Add(nameof(TotalRow), "total_row");
        FieldMap.Add(nameof(CreatedAt), "created_at");
        FieldMap.Add(nameof(UpdatedAt), "updated_at");
        FieldMap.Add(nameof(ProcessUId), "process_uid");
    }

    #endregion Field Config

    protected abstract override void FieldMapper();
}