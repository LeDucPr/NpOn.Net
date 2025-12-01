namespace SSO.ServiceModels.Survey;

public class QuestionGetBySurveyModel : BaseSsoNpOnGrpcModel
{
    public Guid SurveyId { get; set; }
    public string? SurveyTitle { get; set; }
    public string? SurveyDescription { get; set; }
    public bool? SurveyIsPublished { get; set; }
    public DateTime? SurveyCreatedAt { get; set; }
    public DateTime? SurveyExpiredAt { get; set; }
    public DateTime? SurveyUpdatedAt { get; set; }
    
    public Guid? QuestionId { get; set; }
    public string? QuestionQuestionText { get; set; }
    public int? QuestionQuestionOrder { get; set; }
    public bool? QuestionIsRequired { get; set; }
    public DateTime? QuestionCreatedAt { get; set; }
    public DateTime? QuestionUpdatedAt { get; set; }
    
    // Added fields for Question Options
    public Guid? QuestionOptionId { get; set; }
    public string? QuestionOptionCode { get; set; }
    public string? QuestionOptionDescription { get; set; }
    public int? QuestionOptionType { get; set; }

    public Guid? AnswerId { get; set; }
    public string? AnswerDescription { get; set; }
    public int? AnswerOrderSort { get; set; }
    public int? AnswerScore { get; set; }
    public DateTime? AnswerCreatedAt { get; set; }

    protected override void FieldMapper()
    {
        FieldMap?.Add(nameof(SurveyId), "survey_id");
        FieldMap?.Add(nameof(SurveyTitle), "survey_title");
        FieldMap?.Add(nameof(SurveyDescription), "survey_description");
        FieldMap?.Add(nameof(SurveyIsPublished), "survey_is_published");
        FieldMap?.Add(nameof(SurveyCreatedAt), "survey_created_at");
        FieldMap?.Add(nameof(SurveyExpiredAt), "survey_expired_at");
        FieldMap?.Add(nameof(SurveyUpdatedAt), "survey_updated_at");

        FieldMap?.Add(nameof(QuestionId), "question_id");
        FieldMap?.Add(nameof(QuestionQuestionText), "question_question_text");
        FieldMap?.Add(nameof(QuestionQuestionOrder), "question_question_order");
        FieldMap?.Add(nameof(QuestionIsRequired), "question_is_required");
        FieldMap?.Add(nameof(QuestionCreatedAt), "question_created_at");
        FieldMap?.Add(nameof(QuestionUpdatedAt), "question_updated_at");

        // Added mappings for Question Options
        FieldMap?.Add(nameof(QuestionOptionId), "question_option_id");
        FieldMap?.Add(nameof(QuestionOptionCode), "question_option_code");
        FieldMap?.Add(nameof(QuestionOptionDescription), "question_option_description");
        FieldMap?.Add(nameof(QuestionOptionType), "question_option_type");

        FieldMap?.Add(nameof(AnswerId), "answer_id");
        FieldMap?.Add(nameof(AnswerDescription), "answer_description");
        FieldMap?.Add(nameof(AnswerOrderSort), "answer_order_sort");
        FieldMap?.Add(nameof(AnswerScore), "answer_score");
        FieldMap?.Add(nameof(AnswerCreatedAt), "answer_created_at");
    }
}