using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonWebApplication.Services;
using GeneralServiceObject.QueryObjects;
using IGeneralService;
using IQuestionService;
using ProjectEntry.QuestionEntries;
using QuestionServiceObject.QueryObjects;

namespace QuestionService.Services;

public class SurveyService(
    IFldMasterPgService fldMasterPgService,
    ILogger<CommonService> logger
) : CommonService(logger), ISurveyService
{
    public async Task<CommonResponse<INpOnGrpcObject>> GetQuestionsBySurveyId(QuestionGetBySurveyIdQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var surveyGetBy = await fldMasterPgService.Query(new TblFldQuery()
            {
                Code = QuestionServiceQueryCode.QuestionsBySurveyId,
                QueryParams =
                [
                    new TblFldQueryParam()
                    {
                        ParamName = "survey_id",
                        StringValue = query.SurveyId
                    }
                ],
            });

            INpOnGrpcObject? questionGrpTable = surveyGetBy.Data;
            if (!surveyGetBy.Status)
            {
                response.SetFail(surveyGetBy.ErrorMessages);
                return;
            }

            response.Data = questionGrpTable;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<INpOnGrpcObject>> GetQuestionsByUserIdAndSurveyId(
        QuestionGetByUserIdAndSurveyIdQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var surveyGetBy = await fldMasterPgService.Query(new TblFldQuery()
            {
                Code = "sp_dyn_patient_rank_search",
                QueryParams =
                [
                    new TblFldQueryParam()
                    {
                        ParamName = "json_object_data",
                        // StringValue = query.SurveyId
                        StringValue = @"{
                              ""full_name"": """",
                              ""username"": """",
                              ""from_date"": ""2025-11-07T00:00:00"",
                              ""to_date"": ""2025-11-14T23:59:59"",
                              ""mobile_phone"": """",
                              ""gender"": """",
                              ""province_rcd"": """",
                              ""district_rcd"": """",
                              ""commune_rcd"": """",
                              ""standard_account_id"": ""12fbd6a7-978b-4e7f-98bc-43c21684b371"",
                              ""master_account_id"": null,
                              ""province_account_rcd"": """",
                              ""rank_type"": null,
                              ""page"": 1,
                              ""pageSize"": 1
                            }"
                    }
                ],
            });

            INpOnGrpcObject? questionGrpTable = surveyGetBy.Data;
            if (!surveyGetBy.Status)
            {
                response.SetFail(surveyGetBy.ErrorMessages);
                return;
            }

            response.Data = questionGrpTable;
            response.SetSuccess();
        });
    }
}