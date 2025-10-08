using Enums;
using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.CtrlObjs.Connections;

[TableLoader("server_ctrl")]
public class ServerCtrl : BaseCtrl
{
    public string? ServerName { get; set; }
    public EServer? ServerType { get; set; } 
    public string? Host { get; set; }
    public int? Port { get; set; }
}
