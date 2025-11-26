using CommonGrpcObject;
using CommonWebApplication.Services;
using IGeneralService;
using IQuestionService;
using QuestionServiceObject.CommandObjects;
using GeneralServiceObject.QueryObjects;
using Enums;
using QuestionServiceObject.QueryObjects;
using CommonDb.DbResults.Grpc;
using ProjectEnums.FldMasterEnums;
using CommonObject;

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

            // This call is correct as it executes an INSERT/UPDATE
            var addNewSurveyResponse = await fldMasterPgService.Execute(new TblFldExecution()
            {
                Code = addOrUpdateCommand.Id == null ? FldMasterCodes.SurveyAdd : FldMasterCodes.SurveyUpdate,
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
                Code = FldMasterCodes.QuestionsBySurveyId,
                QueryParams = [ new TblFldExecutionParam() { ParamName = "survey_id", StringValue = query.SurveyId } ],
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
    public async Task<CommonResponse<int>> CalculateScore(CalculateSurveyScoreQuery query)
    {
        return await CommonProcess<int>(async (response) =>
        {
            var scoreExecution = new TblFldExecution
            {
                Code = FldMasterCodes.SurveyCalcScore,
                QueryParams =
                [
                    new TblFldExecutionParam { ParamName = "user_id", StringValue = query.UserId },
                    new TblFldExecutionParam { ParamName = "survey_id", StringValue = query.SurveyId }
                ]
            };

            // CORRECTED: Using Execute to run the stored procedure and get the score
            var scoreResult = await fldMasterPgService.Execute(scoreExecution);

            if (!scoreResult.Status || scoreResult.Data == null)
            {
                response.SetFail("Could not calculate score.", scoreResult.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            if (scoreResult.Data is not NpOnGrpcTable table || table.Rows == null || !table.Rows.Any())
            {
                response.SetFail("Score calculation returned no data.", EErrorCode.NotFound);
                return;
            }

            var firstRow = table.Rows.Values.FirstOrDefault();
            var firstCell = firstRow?.Cells.Values.FirstOrDefault();

            if (firstCell == null)
            {
                response.SetFail("Score calculation returned empty cell.", EErrorCode.DataProcessingError);
                return;
            }
            
            var totalScore = firstCell.GetValue<long>();

            response.Data = (int)totalScore;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<INpOnGrpcObject>> GetSurveyOutcomes(string surveyId)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var outcomeExecution = new TblFldExecution
            {
                Code = FldMasterCodes.GetSurveyOutcomesBySurveyId,
                QueryParams = 
                    [
                        new TblFldExecutionParam
                        {
                            ParamName = "ques_srv_survey_id", 
                            StringValue = surveyId
                        }]
            };
            
            // CORRECTED: Using Execute to run the query and get the outcome list
            var outcomesResult = await fldMasterPgService.Execute(outcomeExecution);

            if (!outcomesResult.Status)
            {
                response.SetFail("Could not retrieve survey outcomes.", outcomesResult.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            response.Data = outcomesResult.Data;
            response.SetSuccess();
        });
    }

    public Task<CommonResponse<INpOnGrpcObject>> GetQuestionsByUserIdAndSurveyId(QuestionGetByUserIdAndSurveyIdQuery query)
    {
        throw new NotImplementedException();
    }
}