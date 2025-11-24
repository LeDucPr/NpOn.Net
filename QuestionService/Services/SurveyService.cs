using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonObject;
using CommonWebApplication.Services;
using GeneralServiceObject.QueryObjects;
using IGeneralService;
using IQuestionService;
using ProjectEntry.QuestionEntries;
using QuestionServiceObject.CommandObjects;
using QuestionServiceObject.QueryObjects;

namespace QuestionService.Services;

public class SurveyService(
    IFldMasterPgService fldMasterPgService,
    ILogger<CommonService> logger
) : CommonService(logger), ISurveyService
{
    public async Task<CommonResponse<string>> AddSurvey(SurveyAddCommand command)
    {
        return await CommonProcess<string>(async (response) =>
        {
            var addNewSurveyResponse = await fldMasterPgService.Execute(new TblFldExecution()
            {
                Code = "user_answer_add",
                QueryParams =
                [
                    new TblFldExecutionParam()
                    {
                        ParamName = "title",
                        StringValue = command.Title
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "description",
                        StringValue = command.Description
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "is_published",
                        StringValue = command.IsPublished.AsDefaultString()
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "expired_at",
                        StringValue = command.ExpiredAt.AsDefaultString()
                    },
                ],
            });
            if (!addNewSurveyResponse.Status)
            {
                response.SetFail(addNewSurveyResponse.ErrorMessages);
                return;
            }
            response.Data = "Add new survey success";
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<INpOnGrpcObject>> GetQuestionsBySurveyId(QuestionGetBySurveyIdQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var questionGetBySurveyIdResponse = await fldMasterPgService.Execute(new TblFldExecution()
            {
                Code = QuestionServiceQueryCode.QuestionsBySurveyId,
                QueryParams =
                [
                    new TblFldExecutionParam()
                    {
                        ParamName = "survey_id",
                        StringValue = query.SurveyId
                    }
                ],
            });

            INpOnGrpcObject? questionGrpTable = questionGetBySurveyIdResponse.Data;
            if (!questionGetBySurveyIdResponse.Status)
            {
                response.SetFail(questionGetBySurveyIdResponse.ErrorMessages);
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
            var surveyGetBy = await fldMasterPgService.Execute(new TblFldExecution()
            {
                Code = "sp_dyn_patient_rank_search",
                QueryParams =
                [
                    new TblFldExecutionParam()
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