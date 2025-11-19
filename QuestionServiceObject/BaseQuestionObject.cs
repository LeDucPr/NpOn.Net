using Microsoft.AspNetCore.Http.HttpResults;
using ObjectHandlerFlow.AlgObjs.CtrlObjs;
using ProtoBuf;
using QuestionServiceObject.BusinessObjects;

namespace QuestionServiceObject;

[ProtoContract]
[ProtoInclude(10, typeof(FaqObject))]
[ProtoInclude(100, typeof(QuestionObject))]
[ProtoInclude(200, typeof(SurveysObject))]
[ProtoInclude(300, typeof(AnswerOptionsObject))]
[ProtoInclude(400, typeof(ResultCategoriesObject))]
[ProtoInclude(500, typeof(UsersObject))]
[ProtoInclude(600, typeof(SubmissionAnswersObject))]
public abstract class BaseQuestionObject : BaseCtrl
{
    #region Field Config

    [ProtoMember(1)] public override Dictionary<string, string>? FieldMap { get; protected set; }

    //[ProtoMember(999)] public Guid Id { get; set; }
    //[ProtoMember(1000)] public DateTime CreatedAt { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new();
        //FieldMap ??= new Dictionary<string, string>();
        //FieldMap.Add(nameof(Id), "id");
        //FieldMap.Add(nameof(CreatedAt), "created_at");
    }

    #endregion Field Config
}