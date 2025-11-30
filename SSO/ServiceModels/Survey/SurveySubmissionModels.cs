namespace SSO.ServiceModels.Survey;

public class TotalScoreModel : BaseSsoNpOnGrpcModel
{
    public long? TotalScore { get; set; }

    protected override void FieldMapper()
    {
        FieldMap = new()
        {
            { nameof(TotalScore), "total_score" }
        };
    }
}

public class MaxScoreModel : BaseSsoNpOnGrpcModel
{
    public long? MaxPossibleScore { get; set; }

    protected override void FieldMapper()
    {
        FieldMap = new()
        {
            { nameof(MaxPossibleScore), "max_possible_score" }
        };
    }
}

public class ResultIdModel : BaseSsoNpOnGrpcModel
{
    public string? Id { get; set; }

    protected override void FieldMapper()
    {
        FieldMap = new()
        {
            { nameof(Id), "id" }
        };
    }
}