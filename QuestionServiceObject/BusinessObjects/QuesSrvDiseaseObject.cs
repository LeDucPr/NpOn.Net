using ObjectHandlerFlow.AlgObjs.Attributes;
using ProtoBuf;

namespace QuestionServiceObject.BusinessObjects;

[ProtoContract]
[TableLoader("ques_srv_disease")]
[ProtoInclude(230, typeof(QuesSrvDiseaseObjectDetailObject))]
[ProtoInclude(260, typeof(QuesSrvDiseaseFullObject))]
public class QuesSrvDiseaseObject : BaseQuestionObject
{
    [ProtoMember(1)] public string? Title { get; set; }
    [ProtoMember(2)] public string? Description { get; set; }
    [ProtoMember(3)] public DateTime CreatedAt { get; set; }
    [ProtoMember(4)] public DateTime UpdatedAt { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(Title), "title");
        FieldMap.Add(nameof(Description), "description");
        FieldMap.Add(nameof(CreatedAt), "created_at");
        FieldMap.Add(nameof(UpdatedAt), "updated_at");
        base.FieldMapper();
    }
}

[ProtoContract]
public class QuesSrvDiseaseObjectDetailObject : QuesSrvDiseaseObject
{
    protected override void FieldMapper()
    {
        FieldMap ??= new Dictionary<string, string>();
        base.FieldMapper();
    }
}

//SurveyFullObject
[ProtoContract]
public class QuesSrvDiseaseFullObject : QuesSrvDiseaseObject
{
    [ProtoMember(1)] public List<QuestionWithOptionsObject> Questions { get; set; } = new();
    [ProtoMember(2)] public List<ResultCategoriesObject> ResultCategories { get; set; } = new();

}