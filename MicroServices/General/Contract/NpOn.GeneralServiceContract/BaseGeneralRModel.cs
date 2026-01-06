using Common.Extensions.NpOn.HandleFlow;
using MicroServices.General.Contract.GeneralServiceContract.ReadModels;
using ProtoBuf;

namespace MicroServices.General.Contract.GeneralServiceContract;

[ProtoContract]
[ProtoInclude(100, typeof(TblFldRModel))]
[ProtoInclude(200, typeof(CommandRModel))]
public abstract class BaseGeneralRModel : BaseCtrl
{
    #region Field Config
    [ProtoMember(1)] public override Dictionary<string, string>? FieldMap { get; protected set; }
    protected override void FieldMapper()
    {
        FieldMap ??= new();
    }

    #endregion Field Config
}