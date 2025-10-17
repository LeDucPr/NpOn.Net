using Enums;
using HandlerFlow.AlgObjs.Attributes;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;

namespace HandlerFlow.AlgObjs.CtrlObjs.Data;

[TableLoader("process_table_ctrl")]
public class ProcessTableCtrl : BaseCtrl
{
    public required string ProcessName { get; set; }
    public required EDb DatabaseType { get; set; }
    public required string HandlingString { get; set; } 
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(ProcessName), "process_name");
        FieldMap.Add(nameof(DatabaseType), "database_type");
        FieldMap.Add(nameof(HandlingString), "handling_string");
        FieldMap.Add(nameof(Description), "description");
        FieldMap.Add(nameof(IsActive), "is_active");
    }
}