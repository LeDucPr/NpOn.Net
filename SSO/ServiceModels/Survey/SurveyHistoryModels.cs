namespace SSO.ServiceModels.Survey;

public class SurveyHistoryResultModel : BaseSsoNpOnGrpcModel
{
    public List<SurveyHistoryItemModel>? Items { get; set; }

    public PagingInfoModel? Paging { get; set; }
    
    protected override void FieldMapper()
    {
        // This model is mapped manually from the JSON result, so FieldMap is not used.
    }
}

public class SurveyHistoryItemModel
{
    public string? ResultId { get; set; }

    public string? UserId { get; set; }

    public string? UserFullName { get; set; }

    public string? UserName { get; set; }

    public string? SurveyId { get; set; }

    public string? SurveyTitle { get; set; }

    public int? TotalScore { get; set; }

    public int? MaxPossibleScore { get; set; }

    public object? Outcome { get; set; }

    public DateTime? CompletedAt { get; set; }
}

public class PagingInfoModel
{
    public int TotalRecords { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }
}