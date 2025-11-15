using System.Data;
using Enums;
using ObjectHandlerFlow.AlgObjs.Attributes;

namespace ObjectHandlerFlow.AlgObjs.CtrlObjs.Data;

[TableLoader("table_field_ctrl")]
public class TableFieldCtrl : SysBaseCtrl
{
    [FkId<TableCtrl>(nameof(TableId))] public required long TableId { get; set; }

    [Fk<TableCtrl>(
        $"{nameof(TableFieldCtrl)}.{nameof(TableCtrl)}.{nameof(TableCtrl.Id)}")]
    public TableCtrl? Table { get; set; }

    public required DbType FieldType { get; set; }
    public required EFieldSize FieldSizeType { get; set; }
    public required int Size { get; set; }
    public required string FieldName { get; set; }


    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(TableId), "table_id");
        FieldMap.Add(nameof(FieldName), "field_name");
        FieldMap.Add(nameof(FieldType), "field_type");
        FieldMap.Add(nameof(FieldSizeType), "field_size_type");
        FieldMap.Add(nameof(Size), "size");
    }
}