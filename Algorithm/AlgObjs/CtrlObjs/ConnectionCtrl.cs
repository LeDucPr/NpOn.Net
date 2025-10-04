using Enums;

namespace Algorithm.AlgObjs.CtrlObjs;

public record ConnectionCtrl : BaseCtrl
{
    public string? ConnectionInfoId { get; set; }
    public ConnectionInfoCtrl? ConnectionInfo { get; set; }
    public EDbLanguage? QueryLanguageUse { get; set; }
    public string? Description { get; set; } 
    public bool IsActive { get; set; } 
}
