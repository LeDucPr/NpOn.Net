using Enums;
using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.CtrlObjs.Connections;

[TableLoader("connection_info_ctrl")]
public class ConnectionInfoCtrl : BaseCtrl
{
    [FkId<ServerCtrl>(nameof(ServerId))] public required long ServerId { get; set; }

    [Fk<ServerCtrl>($"{nameof(ConnectionInfoCtrl)}.{nameof(ServerCtrl)}.{nameof(ServerCtrl.Id)}")]
    public ServerCtrl? Server { get; set; }

    public string? DatabaseName { get; set; } // set = keyspace cassandra/scyllaDb
    public EDb? DatabaseType { get; set; }
    public string? Name { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? AuthMethod { get; set; }
    public string? ConnectString { get; set; }
    public bool? SslEnabled { get; set; } // json
    public string? OptionsParams { get; set; } // json


    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(ServerId), "server_id");
        FieldMap.Add(nameof(DatabaseName), "database_name");
        FieldMap.Add(nameof(DatabaseType), "database_type");
        FieldMap.Add(nameof(Name), "name");
        FieldMap.Add(nameof(Username), "username");
        FieldMap.Add(nameof(Password), "password");
        FieldMap.Add(nameof(AuthMethod), "auth_method");
        FieldMap.Add(nameof(ConnectString), "connect_string");
        FieldMap.Add(nameof(SslEnabled), "ssl_enabled");
        FieldMap.Add(nameof(OptionsParams), "options_params");
    }
}