namespace ProjectEnums.FldMasterEnums;

public static class FldMasterCodes
{
    public const string SurveyCreatedAt = "survey_created_at"; // 
    public const string SpDynPatientRankSearch = "sp_dyn_patient_rank_search"; // 
    public const string SurveyCalcScore = "survey_calc_score";
    public const string QuestionsBySurveyId = "questions_by_survey_id";
    public const string SurveyInsertAns = "survey_insert_ans";

    // Codes for Survey Add/Update 
    public const string SurveyAdd = "user_answer_add"; 
    public const string SurveyUpdate = "user_answer_update";

    // Code for fetching Survey Outcomes
    public const string GetSurveyOutcomesBySurveyId = "get_survey_outcomes_by_survey_id_and_score";
}
