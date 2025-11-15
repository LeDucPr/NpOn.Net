using Enums;
using ObjectHandlerFlow.AlgObjs.Attributes;
using ObjectHandlerFlow.AlgObjs.CtrlObjs;
using ProtoBuf;

namespace AccountServiceObject.BusinessObjects;

[ProtoContract]
[TableLoader("server_ctrl")]
public class AccountInfoAliasTestObject : BaseAccountObject
{
    [ProtoMember(1)] public string? ServerName { get; set; }
    [ProtoMember(2)] public required EServer ServerType { get; set; }
    [ProtoMember(3)] public string? Host { get; set; }
    [ProtoMember(4)] public int? Port { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(ServerName), "server_name");
        FieldMap.Add(nameof(ServerType), "server_type");
        FieldMap.Add(nameof(Host), "host");
        FieldMap.Add(nameof(Port), "port");
        base.FieldMapper();
    }
}