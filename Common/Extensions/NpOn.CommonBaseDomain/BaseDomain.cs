using Common.Extensions.NpOn.HandleFlow;
using ProtoBuf;

namespace Common.Extensions.NpOn.CommonBaseDomain;

public interface IBaseDomain;

[ProtoContract]
public class BaseDomain : BaseCtrl, IBaseDomain
{
    public BaseDomain()
    {
    }

    [ProtoMember(1)] public override Dictionary<string, string>? FieldMap { get; protected set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
    }
}