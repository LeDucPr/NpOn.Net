using System.Net;
using QuestionServiceObject.BusinessObjects;
using Newtonsoft.Json.Linq;
using ObjectHandlerFlow.AlgObjs.CtrlObjs;
using ProtoBuf;

namespace QuestionServiceObject;

/// <summary>
/// Cấu trúc riêng, bảng chứa duy nhất 1 cột
/// </summary>
[ProtoContract]
public class BaseQuestionExecFuncJsonObject : BaseCtrl
{
    //Overloads
    public BaseQuestionExecFuncJsonObject()
    {
    }

    public BaseQuestionExecFuncJsonObject(string newJsonFieldName)
    {
        ReplaceJsonFieldMapper(newJsonFieldName);
    }

    #region Field Config

    [ProtoMember(1)] public override Dictionary<string, string>? FieldMap { get; protected set; }

    [ProtoMember(2)] public string? Json { get; set; }

    // For Json Output
    private object? _tObject { get; set; }
    [ProtoMember(3)] public string? Data { get; set; }
    [ProtoMember(4)] public HttpStatusCode? MessageCode { get; set; }
    [ProtoMember(5)] public long? TotalItems { get; set; }
    [ProtoMember(6)] public long? PageCount { get; set; }
    [ProtoMember(7)] public long? PageSize { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new();
        FieldMap.Add(nameof(Json), "json");
    }

    public TObject? ToObject<TObject>()
    {
        if (_tObject != null)
            _tObject = CommonObject.JsonConverter.FromJson<TObject>(Data);
        return (TObject?)_tObject;
    }

    public virtual void ReplaceJsonFieldMapper(string newJsonFieldName)
    {
        if (string.IsNullOrWhiteSpace(newJsonFieldName))
            return;
        FieldMap ??= new();
        FieldMap.TryGetValue(nameof(Json), out string? oldJsonFieldName);
        if (string.IsNullOrEmpty(oldJsonFieldName))
        {
            FieldMap.Remove(nameof(Json));
            FieldMap.Add(nameof(Json), newJsonFieldName);
        }
    }

    #endregion Field Config
}