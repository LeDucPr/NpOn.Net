using System.Data;
using Enums;
using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.CtrlObjs.Data;

[TableLoader("process_field_ctrl")]
public class ProcessFieldCtrl : BaseCtrl
{
    [FkId<ProcessTableCtrl>(nameof(ProcessTableId))] public required long ProcessTableId { get; set; }

    [Fk<ProcessTableCtrl>(
        $"{nameof(ProcessFieldCtrl)}.{nameof(ProcessTable)}.{nameof(ProcessTableCtrl.Id)}")]
    public ProcessTableCtrl? ProcessTable { get; init; }

    public required EDb DatabaseType { get; set; }
    public required string FieldName { get; set; }

    public required DbType FieldType { get; set; } 
    public required EFieldSize FieldSizeType { get; set; }
    public required int Size { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(ProcessTableId), "process_table_id");
        FieldMap.Add(nameof(DatabaseType), "database_type");
        FieldMap.Add(nameof(FieldName), "field_name");
        FieldMap.Add(nameof(FieldType), "field_type");
        FieldMap.Add(nameof(FieldSizeType), "field_size_type");
        FieldMap.Add(nameof(Size), "size");
    }
}