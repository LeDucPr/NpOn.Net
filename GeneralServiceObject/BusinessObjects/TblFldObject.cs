using CommonMode;
using ProtoBuf;

using NpgsqlTypes;
using ProjectEnums.GeneralEnums;

namespace GeneralServiceObject.BusinessObjects;

[ProtoContract]
public class TblFldObject : BaseGeneralObject
{
    [ProtoMember(1)] public Guid? TblMaterId { get; set; }
    [ProtoMember(2)] public string? TblMaterCode { get; set; }
    [ProtoMember(3)] public string? QueryDesc { get; set; }
    [ProtoMember(4)] public string? ExecFunc { get; set; }
    [ProtoMember(5)] public string? Query { get; set; }
    [ProtoMember(6)] public Guid? FldMasterId { get; set; }
    [ProtoMember(7)] public string? FieldName { get; set; }
    [ProtoMember(8)] public string? FieldTypeString { get; set; }
    [ProtoMember(9)] public NpgsqlDbType? FieldType { get; set; }   
    [ProtoMember(10)] public EExecType? ExecType { get; set; }  
    public NpgsqlDbType? FieldDbType => FieldTypeString?.ConvertStringToEnum<NpgsqlDbType>();

    protected override void FieldMapper()
    {
        FieldMap ??= new();
        FieldMap.Add(nameof(TblMaterId), "tbl_id");
        FieldMap.Add(nameof(TblMaterCode), "tbl_code");
        FieldMap.Add(nameof(QueryDesc), "query_desc");
        FieldMap.Add(nameof(ExecFunc), "exec_func");
        FieldMap.Add(nameof(ExecType), "exec_type");
        FieldMap.Add(nameof(Query), "query");
        FieldMap.Add(nameof(FldMasterId), "fld_id");
        FieldMap.Add(nameof(FieldName), "field_name");
        FieldMap.Add(nameof(FieldType), "field_type");
        FieldMap.Add(nameof(FieldTypeString), "field_type_string");
        // base.FieldMapper();
    }
}