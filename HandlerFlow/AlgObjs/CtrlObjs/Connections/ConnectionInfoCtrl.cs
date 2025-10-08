using System.ComponentModel.DataAnnotations.Schema;
using Enums;
using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.CtrlObjs.Connections;

[TableLoader("connection_info")]
public class ConnectionInfoCtrl : BaseCtrl
{
    [FkId<ServerCtrl>(nameof(ServerId))] public string? ServerId { get; set; }

    [Fk<ServerCtrl>($"{nameof(ConnectionInfoCtrl)}.{nameof(ServerCtrl)}.{nameof(ServerCtrl.Id)}")]
    public ServerCtrl? Server { get; set; }

    public string? DatabaseName { get; set; }
    public EDb? DatabaseType { get; set; }
    public string? Name { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? AuthMethod { get; set; }
    public string? ConnectString { get; set; }
    public bool? SslEnabled { get; set; } // json
    public string? OptionsParams { get; set; } // json
}