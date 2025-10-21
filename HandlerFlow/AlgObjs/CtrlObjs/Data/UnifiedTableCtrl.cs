using System.Data;
using Enums;
using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.CtrlObjs.Data;

/// <summary>
/// You can create async that all connections when their driver were
/// Concat all table have in this with condition by Keys
/// </summary>
[TableLoader("table_field_ctrl")]
public class UnifiedTableCtrl : BaseCtrl
{
    public required string UnifiedTableName { get; set; }

    [FkId<TableFieldCtrl>(nameof(TableFieldId))]
    public required long TableFieldId { get; set; }

    [Fk<TableCtrl>(
        $"{nameof(UnifiedTableCtrl)}.{nameof(TableFieldCtrl)}.{nameof(TableFieldCtrl.Id)}")]
    public TableFieldCtrl? TableField { get; set; }

    // process for get data action 
    public string? Description { get; set; }
    public bool IsActive { get; set; }


    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(UnifiedTableName), "unified_table_name");
        FieldMap.Add(nameof(TableFieldId), "table_field_id");
        FieldMap.Add(nameof(Description), "description");
        FieldMap.Add(nameof(IsActive), "is_active");
    }
}