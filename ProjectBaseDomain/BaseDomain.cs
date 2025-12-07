using ObjectHandlerFlow.AlgObjs.CtrlObjs;

namespace ProjectBaseDomain;

public abstract class BaseDomain : BaseCtrl
{
    public override Dictionary<string, string>? FieldMap { get; protected set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
    }
}