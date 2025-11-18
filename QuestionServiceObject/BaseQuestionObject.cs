using ObjectHandlerFlow.AlgObjs.CtrlObjs;
using ProtoBuf;
using QuestionServiceObject.BusinessObjects;

namespace QuestionServiceObject;

[ProtoContract]
[ProtoInclude(100, typeof(FaqObject))]
public abstract class BaseQuestionObject : BaseCtrl
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