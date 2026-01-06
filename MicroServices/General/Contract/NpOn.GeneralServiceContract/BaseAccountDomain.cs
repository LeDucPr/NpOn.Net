using Common.Extensions.NpOn.CommonBaseDomain;
using MicroServices.General.Contract.GeneralServiceContract.Domains;
using ProtoBuf;

namespace MicroServices.General.Contract.GeneralServiceContract;

[ProtoContract]
[ProtoInclude(100, typeof(TblMaster))]
[ProtoInclude(200, typeof(FldQueryMaster))]
public abstract class BaseGeneralDomain : BaseDomain
{
    public BaseGeneralDomain()
    {
    }

    #region Field Config

    public override Dictionary<string, string>? FieldMap { get; protected set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new();
        // FieldMap.Add(nameof(Id), "id");
        // FieldMap.Add(nameof(CreatedAt), "created_at");
    }

    #endregion Field Config
}