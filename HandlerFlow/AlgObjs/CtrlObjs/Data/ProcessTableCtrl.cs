using Enums;
using HandlerFlow.AlgObjs.Attributes;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;

namespace HandlerFlow.AlgObjs.CtrlObjs.Data;

[TableLoader("process_table_ctrl")]
public class ProcessTableCtrl : BaseCtrl
{
    public required string ProcessName { get; set; }
    public required EDb DatabaseType { get; set; }
    public string? HandlingString { get; set; } // Cho phép NULL để khớp với SQL
    public string? NextHandlingString { get; set; } // Thêm để khớp với SQL

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(ProcessName), "process_name");
        FieldMap.Add(nameof(DatabaseType), "database_type");
        FieldMap.Add(nameof(HandlingString), "handling_string");
        FieldMap.Add(nameof(NextHandlingString), "next_handling_string");
    }
}