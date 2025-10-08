using Enums;

namespace HandlerFlow.AlgObjs.CtrlObjs.Connections;

public class ServerCtrl : BaseCtrl
{
    public string? ServerName { get; set; }
    public EServer? ServerType { get; set; } 
    public string? Host { get; set; }
    public int? Port { get; set; }
}
