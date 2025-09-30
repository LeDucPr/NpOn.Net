using Enums;

namespace Algorithm.AlgObjs.CtrlObjs;

public record ConnectionCtrl : BaseCtrl
{
    public string? DatabaseName { get; set; }
    public EDb? DatabaseType { get; set; }
    public EDbLanguage? QueryLanguageUse { get; set; }
    public string? Name { get; set; }
    public string? Host { get; set; }
    public string? Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? AuthMethod { get; set; }
    public string? ConnectString { get; set; }
    public bool? SslEnabled { get; set; } // json
    public string? OptionsParams { get; set; } // json
    public string? Description { get; set; } 
    public bool IsActive { get; set; } 
}
