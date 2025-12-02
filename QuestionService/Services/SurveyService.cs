using CommonGrpcObject;
using CommonWebApplication.Services;
using IGeneralService;
using IQuestionService;
using QuestionServiceObject.CommandObjects;
using GeneralServiceObject.QueryObjects;
using QuestionServiceObject.QueryObjects;
using CommonDb.DbResults.Grpc;
using CommonObject;
using Enums;
using ProjectEntry.QuestionEntries;
using System.Text.Json;
using DbFactory;
using CommonDb.DbResults;
using QuestionServiceObject;

namespace QuestionService.Services;

public class SurveyService(
    IFldMasterPgService fldMasterPgService,
    IDbFactoryWrapper dbFactoryWrapper,
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
                ExecParams = queryParams.ToArray(),
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
            var questionGetBySurveyIdResponse = await fldMasterPgService.Execute(new TblFldExecution()
            {
                Code = QuestionServiceQueryCode.QuestionsBySurveyId,
                ExecParams = [new TblFldExecutionParam() { ParamName = "survey_id", StringValue = query.SurveyId }],
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
                ExecParams =
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
                ExecParams =
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

    public async Task<CommonResponse<INpOnGrpcObject>> GetAnswersScore(AnswersScoreQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var scoreExecution = new TblFldExecution
            {
                Code = QuestionServiceQueryCode.SurveyGetAnswersScore,
                ExecParams = [new TblFldExecutionParam { ParamName = "answer_ids", StringValue = query.AnswerIds }]
            };
            var scoreResponse = await fldMasterPgService.Execute(scoreExecution);
            if (!scoreResponse.Status || scoreResponse.Data == null)
            {
                response.SetFail("Could not calculate score from answers.", scoreResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            response.Data = scoreResponse.Data;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<INpOnGrpcObject>> GetMaxSurveyScore(MaxSurveyScoreQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var maxScoreExecution = new TblFldExecution
            {
                Code = QuestionServiceQueryCode.SurveyGetMaxScore,
                ExecParams =
                [
                    new TblFldExecutionParam { ParamName = "survey_id", StringValue = query.SurveyId }
                ]
            };
            var maxScoreResponse = await fldMasterPgService.Execute(maxScoreExecution);
            if (!maxScoreResponse.Status || maxScoreResponse.Data == null)
            {
                response.SetFail("Could not get max survey score.", maxScoreResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            response.Data = maxScoreResponse.Data;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<BaseQuestionExecFuncJsonObject?>> GetSurveyHistory(SurveyHistoryQuery query)
    {
        return await CommonProcess<BaseQuestionExecFuncJsonObject?>(async (response) =>
        {
            string funcName = "ques_srv_get_by_user_or_survey_history";
            string jsonQuery = JsonSerializer.Serialize(query, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.ExecuteFunc(
                funcName,
                new Dictionary<string, object> { { "json_object_data", jsonQuery } },
                true,
                isUseOutputJsonAsName: funcName
            );

            if (resultOfQuery == null)
            {
                response.SetFail("No result from database.", EErrorCode.NotFound);
                return;
            }

            var historyContainer = resultOfQuery
                .GenericConverterForJson(typeof(BaseQuestionExecFuncJsonObject), jsonColumnName: funcName)?
                .Cast<BaseQuestionExecFuncJsonObject>()
                .FirstOrDefault();
            
            historyContainer?.ToObject<object>();

            response.Data = historyContainer;
            response.SetSuccess();
            
            //// v2
            // string funcCode = "get_by_user_or_survey_history";
            // var funcExecution = new TblFldExecution
            // {
            //     Code = QuestionServiceQueryCode.SurveyGetMaxScore,
            //     QueryParams =
            //     [
            //         new TblFldExecutionParam { ParamName = "survey_id", StringValue = query.SurveyId }
            //     ]
            // };
            //
            // var resultOfQueryResponse = await fldMasterPgService.Execute(funcExecution);
            //
            // if (resultOfQueryResponse.Data == null)
            // {
            //     response.SetFail("No result from database.", EErrorCode.NotFound);
            //     return;
            // }
        });
    }
}