using ObjectHandlerFlow.AlgObjs.Attributes;
using ProtoBuf;

namespace QuestionServiceObject.BusinessObjects;

[ProtoContract]
[TableLoader("result_categories")]
public class ResultCategoriesObject : BaseQuestionObject
{
    [ProtoMember(1)] public Guid SurveyId { get; set; }
    [ProtoMember(2)] public string? CategoryName { get; set; }
    [ProtoMember(3)] public string? Description { get; set; }
    [ProtoMember(4)] public int MinScore { get; set; }
    [ProtoMember(5)] public int MaxScore { get; set; }
    [ProtoMember(6)] public string? Recommendation { get; set; }
    [ProtoMember(7)] public string? SeverityLevel { get; set; }
    [ProtoMember(8)] public string? ColorHex { get; set; }
    [ProtoMember(9)] public int DisplayOrder { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new Dictionary<string, string>();
        FieldMap.Add(nameof(SurveyId), "survey_id");
        FieldMap.Add(nameof(CategoryName), "category_name");
        FieldMap.Add(nameof(Description), "description");
        FieldMap.Add(nameof(MinScore), "min_score");
        FieldMap.Add(nameof(MaxScore), "max_score");
        FieldMap.Add(nameof(Recommendation), "recommendation");
        FieldMap.Add(nameof(SeverityLevel), "severity_level");
        FieldMap.Add(nameof(ColorHex), "color_hex");
        FieldMap.Add(nameof(DisplayOrder), "display_order");
        base.FieldMapper();
    }
}

