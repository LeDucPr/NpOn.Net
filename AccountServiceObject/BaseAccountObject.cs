using AccountServiceObject.BusinessObjects;
using ObjectHandlerFlow.AlgObjs.CtrlObjs;
using ProtoBuf;

namespace AccountServiceObject;

[ProtoContract]
[ProtoInclude(100, typeof(AccountLoginInfoObject))]
[ProtoInclude(200, typeof(AccountInfoAliasTestObject))]
[ProtoInclude(300, typeof(AccountInfoAliasTestObject22222))]
public abstract class BaseAccountObject : BaseCtrl
{
    #region Field Config

    [ProtoMember(1)] public override Dictionary<string, string>? FieldMap { get; protected set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new();
        // FieldMap.Add(nameof(Id), "id");
        // FieldMap.Add(nameof(CreatedAt), "created_at");
    }

    #endregion Field Config
}