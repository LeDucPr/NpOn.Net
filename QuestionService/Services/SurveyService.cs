using CommonGrpcObject;
using CommonWebApplication.Services;
using IGeneralService;
using IQuestionService;
using QuestionServiceObject.CommandObjects;
using GeneralServiceObject.QueryObjects;
using Enums;
using QuestionServiceObject.QueryObjects;
using CommonDb.DbResults.Grpc;
using CommonObject;
using ProjectEntry.QuestionEntries;

namespace QuestionService.Services;

public class SurveyService(
    IFldMasterPgService fldMasterPgService,
    ILogger<CommonService> logger
) : CommonService(logger), ISurveyService
{
    public async Task<CommonResponse<string>> AddOrUpdateSurvey(SurveyAddOrUpdateCommand addOrUpdateCommand)
    {
        return await CommonProcess<string>(async (response) =>
        {
            List<TblFldExecutionParam> queryParams =
            [
                new TblFldExecutionParam() { ParamName = "title", StringValue = addOrUpdateCommand.Title },
                new TblFldExecutionParam() { ParamName = "description", StringValue = addOrUpdateCommand.Description },
                new TblFldExecutionParam() { ParamName = "is_published", StringValue = addOrUpdateCommand.IsPublished.AsDefaultString() },
                new TblFldExecutionParam() { ParamName = "expired_at", StringValue = addOrUpdateCommand.ExpiredAt.AsDefaultString() },
            ];

            if (addOrUpdateCommand.Id != null)
            {
                queryParams.Add(new TblFldExecutionParam() { ParamName = "id", StringValue = addOrUpdateCommand.Id });
            }

            var addNewSurveyResponse = await fldMasterPgService.Execute(new TblFldExecution()
            {
                Code = addOrUpdateCommand.Id == null ? QuestionServiceQueryCode.SurveyAdd : QuestionServiceQueryCode.SurveyUpdate,
                QueryParams = queryParams.ToArray(),
            });

            if (!addNewSurveyResponse.Status)
            {
                response.SetFail(addNewSurveyResponse.ErrorMessages);
                return;
            }

            response.Data = addOrUpdateCommand.Id == null ? "Add new survey success" : "Update survey success";
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<INpOnGrpcObject>> GetQuestionsBySurveyId(QuestionGetBySurveyIdQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            // This call should execute the query to get data
            var questionGetBySurveyIdResponse = await fldMasterPgService.Execute(new TblFldExecution()
            {
                Code = QuestionServiceQueryCode.QuestionsBySurveyId,
                QueryParams = [new TblFldExecutionParam() { ParamName = "survey_id", StringValue = query.SurveyId }],
            });

            if (!questionGetBySurveyIdResponse.Status)
            {
                response.SetFail(questionGetBySurveyIdResponse.ErrorMessages);
                return;
            }

            response.Data = questionGetBySurveyIdResponse.Data;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<INpOnGrpcObject>> CalculateScore(CalculateSurveyScoreQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var scoreExecution = new TblFldExecution
            {
                Code = QuestionServiceQueryCode.SurveyCalcScore,
                QueryParams =
                [
                    new TblFldExecutionParam { ParamName = "user_id", StringValue = query.UserId },
                    new TblFldExecutionParam { ParamName = "survey_id", StringValue = query.SurveyId }
                ]
            };
            var scoreResponse = await fldMasterPgService.Execute(scoreExecution);
            if (!scoreResponse.Status || scoreResponse.Data == null)
            {
                response.SetFail("Could not calculate score.", scoreResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            response.Data = scoreResponse.Data;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<INpOnGrpcObject>> GetSurveyOutcomes(SurveyOutcomeScoreQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var outcomeExecution = new TblFldExecution
            {
                Code = QuestionServiceQueryCode.GetSurveyOutcomesBySurveyId,
                QueryParams =
                [
                    new TblFldExecutionParam
                    {
                        ParamName = "ques_srv_survey_id",
                        StringValue = query.SurveyId
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "total_score",
                        StringValue = query.TotalScore.AsDefaultString(),
                    }
                ]
            };
            var outcomesResult = await fldMasterPgService.Execute(outcomeExecution);
            if (!outcomesResult.Status)
            {
                response.SetFail("Could not retrieve survey outcomes.",
                    outcomesResult.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            response.Data = outcomesResult.Data;
            response.SetSuccess();
        });
    }

    public Task<CommonResponse<INpOnGrpcObject>> GetQuestionsByUserIdAndSurveyId(
        QuestionGetByUserIdAndSurveyIdQuery query)
    {
        throw new NotImplementedException();
    }
}