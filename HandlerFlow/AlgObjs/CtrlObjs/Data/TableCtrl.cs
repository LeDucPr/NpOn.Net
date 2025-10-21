using Enums;
using HandlerFlow.AlgObjs.Attributes;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;

namespace HandlerFlow.AlgObjs.CtrlObjs.Data;

[TableLoader("table_ctrl")]
public class TableCtrl : BaseCtrl
{
    public required string TableName { get; set; }
    public required ETableMode TableMode { get; set; }
    public required EDb DatabaseType { get; set; }


    // connection info
    [FkId<ConnectionInfoCtrl>(nameof(ConnectionInfoId))]
    public required long ConnectionInfoId { get; set; }

    [Fk<ConnectionInfoCtrl>(
        $"{nameof(TableCtrl)}.{nameof(ConnectionInfoCtrl)}.{nameof(ConnectionInfoCtrl.Id)}")]
    public ConnectionInfoCtrl? ConnectionInfo { get; set; }


    // data from other process when this need get by a process
    [FkId<ProcessTableCtrl>(nameof(ProcessTableId))]
    public long ProcessTableId { get; set; }

    [Fk<ProcessTableCtrl>($"{nameof(TableCtrl)}.{nameof(ProcessTableCtrl)}.{nameof(ProcessTableCtrl.Id)}")]
    public ProcessTableCtrl? ProcessTable { get; set; }
    

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(TableName), "table_name");
        FieldMap.Add(nameof(TableMode), "table_mode");
        FieldMap.Add(nameof(DatabaseType), "database_type");
        FieldMap.Add(nameof(ConnectionInfoId), "connection_info_id");
        FieldMap.Add(nameof(ProcessTableId), "process_table_id");
    }
}