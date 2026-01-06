using Common.Extensions.NpOn.CommonBaseDomain.Attributes;
using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.HandleFlow.Attributes;
using ProtoBuf;

namespace MicroServices.General.Contract.GeneralServiceContract.Domains;

[ProtoContract]
[TableLoader("tblmaster")]
public class TblMaster : BaseGeneralDomain
{
    [ProtoMember(1)]
    [Field("id")]
    [Pk("id")]
    public Guid? Id { get; set; }

    [ProtoMember(2)]
    [Field("code")]
    [Pk("code")]
    public string Code { get; set; }

    [ProtoMember(3)]
    [Field("description")]
    public string? Description { get; set; }

    [ProtoMember(4)] [Field("execfunc")] public string? ExecFunc { get; set; }

    [ProtoMember(5)] [Field("query")] public string? Query { get; set; }

    [ProtoMember(6)] [Field("exectype")] public int ExecType { get; set; }

    [ProtoMember(7)] [Field("created_at")] public DateTime CreatedAt { get; set; }

    [ProtoMember(8)]
    [Field("service_name")]
    public string? ServiceName { get; set; }

    [ProtoMember(9)] [Field("db_type")] public EDb? DbType { get; set; }
}