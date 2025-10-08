using System.ComponentModel.DataAnnotations.Schema;
using Enums;
using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.CtrlObjs.Connections;

public class ConnectionCtrl : BaseCtrl
{
    [ForeignKey(nameof(ConnectionInfoId))] public string? ConnectionInfoId { get; set; }

    [Fk<ConnectionInfoCtrl>(
        $"{nameof(ConnectionCtrl)}.{nameof(ConnectionInfoCtrl)}.{nameof(ServerCtrl.Id)}")]
    public ConnectionInfoCtrl? ConnectionInfo { get; set; }

    public EDbLanguage? QueryLanguageUse { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}