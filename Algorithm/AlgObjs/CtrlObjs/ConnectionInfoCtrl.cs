using Enums;

namespace Algorithm.AlgObjs.CtrlObjs;

/// <summary>
/// Được sử dụng cấu hình Database, thay đổi thông tin DB được thiết lập lại tại đây 
/// </summary>
public record ConnectionInfoCtrl : BaseCtrl
{
    public string? ServerId { get; set; }
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