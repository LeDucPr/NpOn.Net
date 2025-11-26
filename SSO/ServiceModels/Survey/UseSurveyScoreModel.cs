namespace SSO.ServiceModels.Survey;

public class UseSurveyScoreModel : BaseSsoNpOnGrpcModel
{
    public long? TotalScore { get; set; }
    protected override void FieldMapper()
    {
        FieldMap?.Add(nameof(TotalScore), "total_score");
    }
}