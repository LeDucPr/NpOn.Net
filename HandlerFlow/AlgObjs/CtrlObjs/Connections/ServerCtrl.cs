using Enums;
using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.CtrlObjs.Connections;

[TableLoader("server_ctrl")]
public class ServerCtrl : BaseCtrl
{
    public string? ServerName { get; set; }
    public required EServer ServerType { get; set; } 
    public string? Host { get; set; }
    public int? Port { get; set; }
    
    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(ServerName), "server_name");
        FieldMap.Add(nameof(ServerType), "server_type");
        FieldMap.Add(nameof(Host), "host");
        FieldMap.Add(nameof(Port), "port");
    }
}
