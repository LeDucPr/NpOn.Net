using Enums;
using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.CtrlObjs.Data;

/// <summary>
/// You can create async that all connections when their driver were
/// Concat all table have in this with condition by Keys
/// </summary>
[TableLoader("table_field_ctrl")]
public class UnifiedTableMappingCtrl : BaseCtrl
{
    [FkId<UnifiedTableCtrl>(nameof(UnifiedTableId))]
    public required long UnifiedTableId { get; set; }

    [Fk<UnifiedTableCtrl>(
        $"{nameof(UnifiedTableMappingCtrl)}.{nameof(UnifiedTableCtrl)}.{nameof(UnifiedTableCtrl.Id)}")]
    public UnifiedTableCtrl? UnifiedTable { get; set; }


    [FkId<TableFieldCtrl>(nameof(TableFieldId))]
    public required long TableFieldId { get; set; }

    [Fk<TableCtrl>(
        $"{nameof(UnifiedTableMappingCtrl)}.{nameof(TableFieldCtrl)}.{nameof(TableFieldCtrl.Id)}")]
    public TableFieldCtrl? TableField { get; set; }

    // process for get data action 
    // join with another table field (TableCtrl) to Create Super Table (UnifiedTableCtrl)
    public EDbJoinType? JoinType { get; set; }
    public long? JoinTableFieldId { get; set; } // the field to join on
    public TableFieldCtrl? JoinTableField { get; set; }
    public int? JoinOrder { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    #region override

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(TableFieldId), "table_field_id");
        FieldMap.Add(nameof(JoinType), "join_type");
        FieldMap.Add(nameof(Description), "description");
        FieldMap.Add(nameof(IsActive), "is_active");
    }

    #endregion override
}