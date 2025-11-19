
using ObjectHandlerFlow.AlgObjs.Attributes;
using ProtoBuf;

namespace QuestionServiceObject.BusinessObjects;

[ProtoContract]
[TableLoader("surveys")]
public class SurveysObject : BaseQuestionObject
{
    [ProtoMember(1)] public string Title { get; set; }
    [ProtoMember(2)] public string Description { get; set; }
    [ProtoMember(3)] public string MaxTotalScore { get; set; }
    [ProtoMember(4)] public bool IsPublished { get; set; }
    [ProtoMember(5)] public DateTime CreatedAt { get; set; }
    [ProtoMember(6)] public DateTime UpdatedAt { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(Title), "title");
        FieldMap.Add(nameof(Description), "description");
        FieldMap.Add(nameof(MaxTotalScore), "max_total_score");
        FieldMap.Add(nameof(IsPublished), "is_published");
        FieldMap.Add(nameof(CreatedAt), "created_at");
        FieldMap.Add(nameof(UpdatedAt), "updated_at");
        base.FieldMapper();
    }
}

[ProtoContract]
public class SurveyDetailObject : SurveysObject
{
    [ProtoMember(1)] public string Title { get; set; }
    [ProtoMember(2)] public string Description { get; set; }
    [ProtoMember(3)] public int MaxTotalScore { get; set; }
    [ProtoMember(4)] public bool IsPublished { get; set; }
    [ProtoMember(5)] public DateTime UpdatedAt { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new Dictionary<string, string>();
        FieldMap.Add(nameof(Title), "title");
        FieldMap.Add(nameof(Description), "description");
        FieldMap.Add(nameof(MaxTotalScore), "max_total_score");
        FieldMap.Add(nameof(IsPublished), "is_published");
        FieldMap.Add(nameof(UpdatedAt), "updated_at");
        base.FieldMapper();
    }
}

[ProtoContract]
public class SurveyFullObject : SurveysObject
{
    [ProtoMember(6)] public List<QuestionWithOptionsObject> Questions { get; set; }
    [ProtoMember(7)] public List<ResultCategoriesObject> ResultCategories { get; set; }

    public SurveyFullObject()
    {
        Questions = new List<QuestionWithOptionsObject>();
        ResultCategories = new List<ResultCategoriesObject>();
    }
}
