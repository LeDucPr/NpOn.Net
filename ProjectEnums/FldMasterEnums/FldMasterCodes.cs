namespace ProjectEnums.FldMasterEnums;

/// <summary>
/// Contains constant strings that map directly to the 'code' column in the 'tblmaster' table.
/// This ensures type-safety and avoids magic strings when calling the data access layer.
/// </summary>
public static class FldMasterCodes
{
    // Existing codes from tblmaster
    public const string SurveyCreatedAt = "survey_created_at";
    public const string SpDynPatientRankSearch = "sp_dyn_patient_rank_search";
    public const string GetOptionsByQuestion = "get_options_by_question";
    public const string SurveyCalcScore = "survey_calc_score";
    public const string QuestionQuestionOrder = "question_question_order";
    public const string UserTotalScore = "user_total_score";
    public const string QuestionsAnswerUser = "questions_answer_user";
    public const string HistoryCreatedAt = "history_created_at";
    public const string QuestionsBySurveyId = "questions_by_survey_id";
    public const string SurveyInsertAns = "survey_insert_ans";
    public const string GetAnswerOptions = "get_answer_options";

    // Codes for Survey Add/Update (mapping to existing tblmaster entries with potentially misleading names)
    // NOTE: "user_answer_add" in tblmaster actually inserts into ques_srv_survey
    public const string SurveyAdd = "user_answer_add"; 
    // NOTE: "user_answer_update" in tblmaster actually updates ques_srv_survey
    public const string SurveyUpdate = "user_answer_update";

    // Code for fetching Survey Outcomes (requires a new entry in tblmaster)
    public const string GetSurveyOutcomesBySurveyId = "get_survey_outcomes_by_survey_id";
}
