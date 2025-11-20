using GeneralServiceObject.BusinessObjects;
using ObjectHandlerFlow.AlgObjs.CtrlObjs;
using ProtoBuf;

namespace GeneralServiceObject;

[ProtoContract]
[ProtoInclude(100, typeof(TblFldObject))]
public abstract class BaseGeneralObject : BaseCtrl
{
    #region Field Config
    [ProtoMember(1)] public override Dictionary<string, string>? FieldMap { get; protected set; }
    protected override void FieldMapper()
    {
        FieldMap ??= new();
    }

    #endregion Field Config
}