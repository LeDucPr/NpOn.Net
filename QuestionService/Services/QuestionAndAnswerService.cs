using CommonGrpcObject;
using CommonObject;
using CommonWebApplication.Services;
using GeneralServiceObject.QueryObjects;
using IGeneralService;
using IQuestionService;
using ProjectEntry.QuestionEntries;
using QuestionServiceObject.CommandObjects;

namespace QuestionService.Services;

public class QuestionAndAnswerService(
    IFldMasterPgService fldMasterPgService,
    ILogger<CommonService> logger
) : CommonService(logger), IQuestionAndAnswerService
{
    public async Task<CommonResponse<string>> SubmitAnswers(SubmitSurveyCommand command)
    {
        return await CommonProcess<string>(async (response) =>
        {
            foreach (var answer in command.Answers)
            {
                var queryParams = new List<TblFldExecutionParam>
                {
                    new() { ParamName = "user_id", StringValue = command.UserId.AsDefaultString() },
                    new() { ParamName = "question_id", StringValue = answer.QuestionId.AsDefaultString() },
                    new() { ParamName = "answer_ids", StringValue = answer.AnswerIds?.AsArrayJoin() },
                    new() { ParamName = "text_answer", StringValue = answer.TextAnswer }
                };

                // This call is correct as it executes an INSERT
                var result = await fldMasterPgService.Execute(new TblFldExecution
                {
                    Code = QuestionServiceQueryCode.SurveyInsertAns,
                    QueryParams = queryParams.ToArray()
                });

                if (!result.Status)
                {
                    response.Status = false;
                    response.ErrorCode = result.ErrorCode;
                    response.Data = $"Failed to submit answer for question {answer.QuestionId}.";
                    return;
                }
            }

            response.Status = true;
            response.Data = "All answers submitted successfully.";
        });
    }
    
}