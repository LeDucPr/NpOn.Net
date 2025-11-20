using CommonMode;
using ProtoBuf;

using NpgsqlTypes;

namespace GeneralServiceObject.BusinessObjects;

[ProtoContract]
public class TblFldObject : BaseGeneralObject
{
    [ProtoMember(1)] public string? TblMaterId { get; set; }
    [ProtoMember(2)] public string? TblMaterCode { get; set; }
    [ProtoMember(3)] public string? QueryDesc { get; set; }
    [ProtoMember(4)] public string? ExecFunc { get; set; }
    [ProtoMember(5)] public string? Query { get; set; }
    [ProtoMember(6)] public string? FldMasterId { get; set; }
    [ProtoMember(7)] public string? FieldName { get; set; }
    [ProtoMember(8)] public string? FieldType { get; set; }
    public NpgsqlDbType? FieldDbType => FieldType?.ConvertStringToEnum<NpgsqlDbType>();

    protected override void FieldMapper()
    {
        FieldMap ??= new();
        FieldMap.Add(nameof(TblMaterId), "tbl_id");
        FieldMap.Add(nameof(TblMaterCode), "tbl_code");
        FieldMap.Add(nameof(QueryDesc), "query_desc");
        FieldMap.Add(nameof(ExecFunc), "exec_func");
        FieldMap.Add(nameof(Query), "query");
        FieldMap.Add(nameof(FldMasterId), "fld_id");
        FieldMap.Add(nameof(FieldName), "field_name");
        FieldMap.Add(nameof(FieldType), "field_type");
        base.FieldMapper();
    }
}