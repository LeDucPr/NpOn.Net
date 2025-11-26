using CommonGrpcObject;
using CommonObject;
using CommonWebApplication.Services;
using Enums;
using IQuestionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionServiceObject.CommandObjects;
using QuestionServiceObject.QueryObjects;
using SSO.Mappings.Survey;
using SSO.Requests;
using SSO.ServiceModels;
using SSO.ServiceModels.Survey;
using SSO.OutputModels;

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
    [AllowAnonymous]
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
    [AllowAnonymous]
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
            var scoreResponse = await surveyService.CalculateScore(new CalculateSurveyScoreQuery
            {
                UserId = "61bf3d62-cc1d-49ca-b5bc-e19634e9b0fa",
                SurveyId = request.SurveyId
            });

            if (!scoreResponse.Status)
            {
                response.SetFail(scoreResponse.ErrorMessages);
                return;
            }

            UseSurveyScoreModel? scoreModel = scoreResponse.Data
                ?.ConverterToChildOfSsoModel(typeof(UseSurveyScoreModel))?
                .OfType<UseSurveyScoreModel>().FirstOrDefault();

            int totalScore = scoreModel?.TotalScore?.AsDefaultInt() ?? 2;

            // compare
            var outcomesResponse = await surveyService.GetSurveyOutcomes(new SurveyOutcomeScoreQuery()
            {
                SurveyId = request.SurveyId,
                TotalScore = totalScore
            });
            if (!outcomesResponse.Status)
            {
                response.SetFail(outcomesResponse.ErrorMessages);
                return;
            }

            // Manually convert the raw INpOnGrpcObject to a list of SurveyOutcomeObject
            List<SurveyComeoutScoreModel>? outcomeModels = outcomesResponse.Data
                ?.ConverterToChildOfSsoModel(typeof(SurveyComeoutScoreModel))?
                .OfType<SurveyComeoutScoreModel>().ToList();

            if (outcomeModels is not { Count: > 0 })
            {
                response.SetFail("No outcomes configured for this survey.", EErrorCode.NotFound);
                return;
            }

            // Find the matching outcome in the controller
            SurveyScoreOutcomeOutputModel[] finalOutcomes = outcomeModels.Select(x => x.ToModel()).ToArray();

            response.Data = finalOutcomes;
            response.SetSuccess();
        });
    }
}