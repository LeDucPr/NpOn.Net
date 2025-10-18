using System.ComponentModel.DataAnnotations.Schema;
using Enums;
using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.CtrlObjs.Connections;

[TableLoader("connection_ctrl")]
public class ConnectionCtrl : BaseCtrl
{
    [FkId<ConnectionInfoCtrl>(nameof(ConnectionInfoId))]
    public required long ConnectionInfoId { get; set; }

    [Fk<ConnectionInfoCtrl>(
        $"{nameof(ConnectionCtrl)}.{nameof(ConnectionInfoCtrl)}.{nameof(ServerCtrl.Id)}")]
    public ConnectionInfoCtrl? ConnectionInfo { get; set; }

    public required EDbLanguage QueryLanguageUse { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(ConnectionInfoId), "connection_info_id");
        FieldMap.Add(nameof(QueryLanguageUse), "query_language_use");
        FieldMap.Add(nameof(Description), "description");
        FieldMap.Add(nameof(IsActive), "is_active");
    }
}