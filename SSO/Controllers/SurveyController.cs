using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonWebApplication.Services;
using Enums;
using IQuestionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionServiceObject.BusinessObjects;
using QuestionServiceObject.QueryObjects;
using SSO.Mappings.Survey;
using SSO.OutputModels;
using SSO.Requests;
using SSO.ServiceModels;
using SSO.ServiceModels.Survey;

namespace SSO.Controllers;

public class SurveyController(
    ILogger<AccountController> logger,
    ContextService contextService,
    ISurveyService surveyService
) : BaseSsoController(logger, contextService)
{
    /// <summary>
    /// API 1: Lấy toàn bộ survey với full question và answer
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    public async Task<CommonApiResponse<object>> QuestionGetBySurvey(
        [FromBody] QuestionGetBySurveyIdRequest? request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            if (request == null)
            {
                response.SetFail(EErrorCode.NullRequestExceptions);
                return;
            }

            var surveyGetBy = await surveyService.GetQuestionsBySurveyId(new QuestionGetBySurveyIdQuery()
            {
                SurveyId = request.SurveyId
            });

            INpOnGrpcObject? questions = surveyGetBy.Data;
            if (!surveyGetBy.Status)
            {
                response.SetFail(surveyGetBy.ErrorMessages);
                return;
            }

            List<QuestionGetBySurveyModel>? questionGetBySurveyModels = questions
                ?.ConverterToChildOfSsoModel(typeof(QuestionGetBySurveyModel))?
                .Cast<QuestionGetBySurveyModel>()
                .ToList();


            SurveyModel? outputModel = questionGetBySurveyModels.ToSurveyModel();

            response.Data = new
            {
                Models = outputModel,
            };
            response.SetSuccess();
        });
    }

    /// <summary>
    /// API 2: Submit survey responses sau khi user lựa chọn
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<CommonApiResponse<object>> SubmitSurveyResponse(
        [FromBody] SubmitSurveyRequest? request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            if (request == null)
            {
                response.SetFail(EErrorCode.NullRequestExceptions);
                return;
            }

            // Get user info if available
            var userInfo = contextService.UserInfo();
            var userId = request.UserId ?? userInfo?.Id.ToString();

            // Calculate total score first
            var calculateScoreQuery = new CalculateSurveyScoreQuery
            {
                SurveyId = request.SurveyId,
                Answers = request.Answers.Select(a => new SubmitAnswerQuery
                {
                    QuestionId = a.QuestionId,
                    SelectedOptionIds = a.SelectedOptionIds ?? []
                }).ToList()
            };

            var scoreResult = await surveyService.CalculateSurveyScore(calculateScoreQuery);
            if (!scoreResult.Status)
            {
                response.SetFail(scoreResult.ErrorMessages);
                return;
            }

            // Submit survey with calculated score
            var submitCommand = new QuestionServiceObject.CommandObjects.SubmitSurveyCommand
            {
                SurveyId = request.SurveyId,
                UserId = userId,
                Answers = request.Answers.Select((a, index) => new QuestionServiceObject.CommandObjects.SubmitAnswerCommand
                {
                    QuestionId = a.QuestionId,
                    TextAnswer = a.TextAnswer,
                    SelectedOptionIds = a.SelectedOptionIds ?? [],
                    ScoreEarned = scoreResult.Data?.QuestionScores[index].ScoreEarned ?? 0
                }).ToList(),
                TotalScore = scoreResult.Data?.TotalScore ?? 0,
                SubmittedAt = DateTime.UtcNow
            };

            var submitResult = await surveyService.SubmitSurvey(submitCommand);
            if (!submitResult.Status)
            {
                response.SetFail(submitResult.ErrorMessages);
                return;
            }

            response.Data = new
            {
                Message = submitResult.Data,
                Score = scoreResult.Data?.TotalScore,
                ResultCategory = scoreResult.Data?.ResultCategory
            };
            response.SetSuccess();
        });
    }

    /// <summary>
    /// API 3: Tính điểm sau khi user lựa chọn
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    public async Task<CommonApiResponse<object>> CalculateSurveyScore(
        [FromBody] CalculateSurveyScoreRequest? request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            if (request == null)
            {
                response.SetFail(EErrorCode.NullRequestExceptions);
                return;
            }

            var calculateQuery = new CalculateSurveyScoreQuery
            {
                SurveyId = request.SurveyId,
                Answers = request.Answers.Select(a => new SubmitAnswerQuery
                {
                    QuestionId = a.QuestionId,
                    SelectedOptionIds = a.SelectedOptionIds ?? []
                }).ToList()
            };

            var scoreResult = await surveyService.CalculateSurveyScore(calculateQuery);
            if (!scoreResult.Status)
            {
                response.SetFail(scoreResult.ErrorMessages);
                return;
            }

            response.Data = new
            {
                TotalScore = scoreResult.Data?.TotalScore,
                ResultCategory = scoreResult.Data?.ResultCategory,
                QuestionScores = scoreResult.Data?.QuestionScores?.Select(qs => new
                {
                    QuestionId = qs.QuestionId,
                    ScoreEarned = qs.ScoreEarned,
                    MaxScore = qs.MaxScore
                }).ToList()
            };
            response.SetSuccess();
        });
    }
}