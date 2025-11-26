using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonWebApplication.Services;
using Enums;
using IQuestionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionServiceObject.BusinessObjects;
using QuestionServiceObject.CommandObjects;
using QuestionServiceObject.QueryObjects;
using SSO.Mappings.Survey;
using SSO.Requests;
using SSO.ServiceModels;
using SSO.ServiceModels.Survey;
using Newtonsoft.Json;

namespace SSO.Controllers;

public class SurveyController(
    ILogger<AccountController> logger,
    ContextService contextService,
    IQuestionAndAnswerService questionAndAnswerService,
    ISurveyService surveyService)
    : BaseSsoController(logger, contextService)
{
    private readonly ContextService _contextService = contextService;

    /// <summary>
    /// API 1: Lấy toàn bộ survey với full question và answer
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    public async Task<CommonApiResponse<object>> GetSurveyDetail([FromBody] QuestionGetBySurveyIdRequest? request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            if (request == null)
            {
                response.SetFail("Request cannot be null.", EErrorCode.NullRequestExceptions);
                return;
            }

            var surveyGetResponse = await surveyService.GetQuestionsBySurveyId(new QuestionGetBySurveyIdQuery()
            {
                SurveyId = request.SurveyId
            });

            if (!surveyGetResponse.Status)
            {
                response.SetFail(surveyGetResponse.ErrorMessages);
                return;
            }

            List<QuestionGetBySurveyModel>? models = surveyGetResponse.Data
                ?.ConverterToChildOfSsoModel(typeof(QuestionGetBySurveyModel))?
                .OfType<QuestionGetBySurveyModel>()
                .ToList();

            response.Data = new { Models = models.ToSurveyModel() };
            response.SetSuccess();
        });
    }

    /// <summary>
    /// API 2: User gửi các câu trả lời của một bài khảo sát
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<CommonApiResponse<object>> SubmitAnswers([FromBody] SubmitSurveyRequest? request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            if (request == null)
            {
                response.SetFail("Request cannot be null.", EErrorCode.NullRequestExceptions);
                return;
            }

            var userId = _contextService.GetSessionKey();
            if (string.IsNullOrEmpty(userId))
            {
                response.SetFail("User is not authenticated or session key is missing.", EErrorCode.UserNotFound);
                return;
            }

            var command = new SubmitSurveyCommand
            {
                UserId = userId,
                SurveyId = request.SurveyId,
                Answers = request.Answers.Select(a => new SubmissionAnswer
                {
                    QuestionId = a.QuestionId,
                    AnswerIds = a.SelectedOptionIds ?? new List<string>(),
                    TextAnswer = a.TextAnswer
                }).ToList()
            };

            var submitResult = await questionAndAnswerService.SubmitAnswers(command);

            if (!submitResult.Status)
            {
                response.SetFail(submitResult.ErrorMessages);
                return;
            }

            response.Data = new { Message = submitResult.Data };
            response.SetSuccess();
        });
    }

    /// <summary>
    /// API 3: Tính điểm và lấy kết quả cuối cùng của một bài khảo sát cho user
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<CommonApiResponse<object>> GetSurveyOutcome([FromBody] GetSurveyOutcomeRequest? request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            if (request == null)
            {
                response.SetFail("Request cannot be null.", EErrorCode.NullRequestExceptions);
                return;
            }

            var userId = _contextService.GetSessionKey();
            if (string.IsNullOrEmpty(userId))
            {
                response.SetFail("User is not authenticated or session key is missing.", EErrorCode.UserNotFound);
                return;
            }

            // Step 1: Calculate score
            var scoreQuery = new CalculateSurveyScoreQuery
            {
                UserId = userId,
                SurveyId = request.SurveyId
            };
            var scoreResponse = await surveyService.CalculateScore(scoreQuery);

            if (!scoreResponse.Status)
            {
                response.SetFail(scoreResponse.ErrorMessages);
                return;
            }

            var totalScore = scoreResponse.Data;

            // Step 2: Get possible outcomes as raw data
            if (request.SurveyId != null)
            {
                var outcomesResponse = await surveyService.GetSurveyOutcomes(request.SurveyId);
                if (!outcomesResponse.Status)
                {
                    response.SetFail(outcomesResponse.ErrorMessages);
                    return;
                }

                // Step 3: Manually convert the raw INpOnGrpcObject to a list of SurveyOutcomeObject
                var outcomes = new List<SurveyOutcomeObject>();
                if (outcomesResponse.Data is NpOnGrpcTable { Rows: not null } table)
                {
                    foreach (var row in table.Rows.Values)
                    {
                        // Convert row cells to a dictionary
                        var rowDict = row.Cells.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetValue<object>());
                        // Serialize dictionary to JSON, then deserialize to the target object
                        var json = JsonConvert.SerializeObject(rowDict);
                        var outcome = JsonConvert.DeserializeObject<SurveyOutcomeObject>(json);
                        if (outcome != null)
                        {
                            outcomes.Add(outcome);
                        }
                    }
                }

                if (!outcomes.Any())
                {
                    response.SetFail("No outcomes configured for this survey.", EErrorCode.NotFound);
                    return;
                }

                // Step 4: Find the matching outcome in the controller
                SurveyOutcomeObject? finalOutcome = outcomes
                    .FirstOrDefault(o => totalScore >= o.MinScore && (o.MaxScore == null || totalScore <= o.MaxScore));

                if (finalOutcome == null)
                {
                    response.SetFail("No matching outcome found for the calculated score.", EErrorCode.NotFound);
                    return;
                }

                response.Data = finalOutcome;
            }

            response.SetSuccess();
        });
    }
}