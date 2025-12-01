using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonObject;
using CommonWebApplication.Services;
using Enums;
using IGeneralService;
using IQuestionService;
using QuestionServiceObject.CommandObjects;
using GeneralServiceObject.QueryObjects;
using ProjectEntry.QuestionEntries;

namespace QuestionService.Services;

public class QuestionAndAnswerService(
    IFldMasterPgService fldMasterPgService,
    ILogger<CommonService> logger
) : CommonService(logger), IQuestionAndAnswerService
{
    public async Task<CommonResponse<string>> InsertUserAnswer(List<UserAnswerSubmitCommand> commands)
    {
        return await CommonProcess<string>(async (response) =>
        {
            foreach (var command in commands)
            {
                var answerExecution = new TblFldExecution
                {
                    Code = QuestionServiceQueryCode.SurveyInsertUserAnswer,
                    QueryParams =
                    [
                        new TblFldExecutionParam { ParamName = "user_id", StringValue = command.UserId },
                        new TblFldExecutionParam { ParamName = "question_id", StringValue = command.QuestionId },
                        new TblFldExecutionParam { ParamName = "answer_ids", StringValue = "{" + string.Join(",", command.AnswerIds) + "}" },
                        new TblFldExecutionParam { ParamName = "text_answer", StringValue = command.TextAnswer.AsDefaultString() },
                        new TblFldExecutionParam { ParamName = "score_text_answer", StringValue = command.ScoreTextAnswer.AsDefaultString() },
                        new TblFldExecutionParam { ParamName = "result_id", StringValue = command.ResultId }
                    ]
                };
                var answerResponse = await fldMasterPgService.Execute(answerExecution);
                if (!answerResponse.Status)
                {
                    response.SetFail($"Failed to submit answer for question {command.QuestionId}.", answerResponse.ErrorCode ?? EErrorCode.Fail);
                    return;
                }
            }

            response.Data = "All answers submitted successfully.";
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<INpOnGrpcObject>> InsertUserResult(SurveyResultInsertCommand command)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var insertExecution = new TblFldExecution
            {
                Code = QuestionServiceQueryCode.SurveyResultInsert,
                QueryParams =
                [
                    new TblFldExecutionParam { ParamName = "user_id", StringValue = command.UserId },
                    new TblFldExecutionParam { ParamName = "survey_id", StringValue = command.SurveyId },
                    new TblFldExecutionParam { ParamName = "total_score", StringValue = command.TotalScore.AsDefaultString() },
                    new TblFldExecutionParam { ParamName = "max_score", StringValue = command.MaxScore.AsDefaultString() },
                    new TblFldExecutionParam { ParamName = "outcome_data", StringValue = command.OutcomeData }
                ]
            };
            var insertResponse = await fldMasterPgService.Execute(insertExecution);
            if (!insertResponse.Status || insertResponse.Data == null)
            {
                response.SetFail("Could not insert survey result.", insertResponse.ErrorCode ?? EErrorCode.Fail);
                return;
            }

            response.Data = insertResponse.Data;
            response.SetSuccess();
        });
    }
}