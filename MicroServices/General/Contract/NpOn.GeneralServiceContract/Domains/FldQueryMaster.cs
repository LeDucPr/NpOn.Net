using Common.Extensions.NpOn.CommonBaseDomain.Attributes;
using Common.Extensions.NpOn.HandleFlow.Attributes;
using ProtoBuf;

namespace MicroServices.General.Contract.GeneralServiceContract.Domains;

[ProtoContract]
[TableLoader("fld_query_master")]
public class FldQueryMaster : BaseGeneralDomain
{
    [ProtoMember(1)]
    [Field("id")]
    [Pk("id")]
    public Guid Id { get; set; }

    [ProtoMember(2)]
    [Field("description")]
    public string? Description { get; set; }

    [ProtoMember(3)]
    [Field("tblmaster_id")]
    [Pk("tblmaster_id")]
    public Guid TblMasterId { get; set; }

    [ProtoMember(4)]
    [Field("field_name")]
    [Pk("field_name")]
    public string FieldName { get; set; }

    [ProtoMember(5)] [Field("field_type")] public int? FieldType { get; set; } // Enum 

    [ProtoMember(6)] [Field("created_at")] public DateTime CreatedAt { get; set; }

    [ProtoMember(7)]
    [Field("field_type_string")]
    public string? FieldTypeString { get; set; }

    [ProtoMember(8)] [Field("order_sort")] public int? OrderSort { get; set; }
}