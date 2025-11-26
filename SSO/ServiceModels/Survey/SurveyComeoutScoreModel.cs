namespace SSO.ServiceModels.Survey;

public class SurveyComeoutScoreModel : BaseSsoNpOnGrpcModel
{
    public required Guid Id { get; set; }
    public required Guid QuesSrvSurveyId { get; set; }
    public int MinScore { get; set; }
    public int MaxScore { get; set; }
    public string? ConditionLabel { get; set; }
    public string? ResultTitle { get; set; }
    public string? ResultDescription { get; set; }
    public string? Recommendation { get; set; }

    protected override void FieldMapper()
    {
        FieldMap?.Add(nameof(Id), "id");
        FieldMap?.Add(nameof(QuesSrvSurveyId), "ques_srv_survey_id");
        FieldMap?.Add(nameof(MinScore), "min_score");
        FieldMap?.Add(nameof(MaxScore), "max_score");
        FieldMap?.Add(nameof(ConditionLabel), "condition_label");
        FieldMap?.Add(nameof(ResultTitle), "result_title");
        FieldMap?.Add(nameof(ResultDescription), "result_description");
        FieldMap?.Add(nameof(Recommendation), "recommendation");
    }
}