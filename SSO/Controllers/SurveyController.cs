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
using System.Text.Json;

namespace SSO.Controllers;

public class SurveyController(
    ILogger<AccountController> logger,
    ContextService contextService,
    IQuestionAndAnswerService questionAndAnswerService,
    ISurveyService surveyService)
    : BaseSsoController(logger, contextService)
{
    private readonly ContextService _contextService = contextService;
    
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

    [AllowAnonymous]
    [HttpPost]
    public async Task<CommonApiResponse<object>> SubmitSurvey([FromBody] SubmitSurveyRequest? request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            if (request?.Answers == null)
            {
                response.SetFail("Request or answers cannot be null.", EErrorCode.NullRequestExceptions);
                return;
            }

            //var userId = _contextService.GetSessionKey();
            var userId = "00353bde-b7df-4c9e-a6fd-d6ecda162972";
            if (string.IsNullOrEmpty(userId))
            {
                response.SetFail("User is not authenticated or session key is missing.", EErrorCode.UserNotFound);
                return;
            }

            var allAnswerIds = request.Answers.Where(a => a.SelectedOptionIds != null).SelectMany(a => a.SelectedOptionIds!).ToArray();
            long totalScore = 0;

            if (allAnswerIds.Length > 0)
            {
                var scoreResponse = await surveyService.GetAnswersScore(new AnswersScoreQuery { AnswerIds = string.Join(",", allAnswerIds) });
                if (!scoreResponse.Status)
                {
                    response.SetFail(scoreResponse.ErrorMessages);
                    return;
                }
                var scoreModel = scoreResponse.Data?.ConverterToChildOfSsoModel(typeof(TotalScoreModel))?.OfType<TotalScoreModel>().FirstOrDefault();
                totalScore += scoreModel?.TotalScore ?? 0;
            }

            totalScore += request.Answers.Sum(a => a.ScoreTextAnswer ?? 0);

            var maxScoreResponse = await surveyService.GetMaxSurveyScore(new MaxSurveyScoreQuery { SurveyId = request.SurveyId });
            if (!maxScoreResponse.Status)
            {
                response.SetFail(maxScoreResponse.ErrorMessages);
                return;
            }
            var maxScoreModel = maxScoreResponse.Data?.ConverterToChildOfSsoModel(typeof(MaxScoreModel))?.OfType<MaxScoreModel>().FirstOrDefault();
            long maxScore = maxScoreModel?.MaxPossibleScore ?? 0;

            var outcomesResponse = await surveyService.GetSurveyOutcomes(new SurveyOutcomeScoreQuery { SurveyId = request.SurveyId, TotalScore = totalScore });
            if (!outcomesResponse.Status)
            {
                response.SetFail(outcomesResponse.ErrorMessages);
                return;
            }
            var outcomeModel = outcomesResponse.Data?.ConverterToChildOfSsoModel(typeof(SurveyComeoutScoreModel))?.OfType<SurveyComeoutScoreModel>().FirstOrDefault();
            var outcomeJson = outcomeModel != null ? JsonSerializer.Serialize(outcomeModel) : "{}";

            var resultInsertCmd = new SurveyResultInsertCommand
            {
                UserId = userId,
                SurveyId = request.SurveyId,
                TotalScore = totalScore,
                MaxScore = maxScore,
                OutcomeData = outcomeJson
            };
            var resultInsertResponse = await questionAndAnswerService.InsertUserResult(resultInsertCmd);
            if (!resultInsertResponse.Status)
            {
                response.SetFail(resultInsertResponse.ErrorMessages);
                return;
            }
            var resultIdModel = resultInsertResponse.Data?.ConverterToChildOfSsoModel(typeof(ResultIdModel))?.OfType<ResultIdModel>().FirstOrDefault();
            var resultId = resultIdModel?.Id;

            if (string.IsNullOrEmpty(resultId))
            {
                response.SetFail("Failed to retrieve result ID after insertion.", EErrorCode.Fail);
                return;
            }

            var answerCommands = request.Answers.Select(a => new UserAnswerSubmitCommand
            {
                UserId = userId,
                QuestionId = a.QuestionId,
                AnswerIds = a.SelectedOptionIds?.ToArray() ?? [],
                TextAnswer = a.TextAnswer,
                ScoreTextAnswer = a.ScoreTextAnswer,
                ResultId = resultId
            }).ToList();

            var submitResult = await questionAndAnswerService.InsertUserAnswer(answerCommands);
            if (!submitResult.Status)
            {
                response.SetFail(submitResult.ErrorMessages);
                return;
            }

            response.Data = new
            {
                ResultId = resultId,
                TotalScore = totalScore,
                MaxPossibleScore = maxScore,
                Outcome = outcomeModel?.ToModel()
            };
            response.SetSuccess();
        });
    }
    
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

            // Calculate score
            var scoreResponse = await surveyService.CalculateScore(new CalculateSurveyScoreQuery
            {
                UserId = userId,
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

            int totalScore = scoreModel?.TotalScore?.AsDefaultInt() ?? 0;

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
            
            List<SurveyComeoutScoreModel>? outcomeModels = outcomesResponse.Data
                ?.ConverterToChildOfSsoModel(typeof(SurveyComeoutScoreModel))?
                .OfType<SurveyComeoutScoreModel>().ToList();

            if (outcomeModels is not { Count: > 0 })
            {
                response.SetFail("No outcomes configured for this survey.", EErrorCode.NotFound);
                return;
            }

            SurveyScoreOutcomeOutputModel[] finalOutcomes = outcomeModels.Select(x => x.ToModel()).ToArray();

            response.Data = finalOutcomes;
            response.SetSuccess();
        });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<CommonApiResponse<object>> GetSurveyHistory([FromBody] GetSurveyHistoryRequest? request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            if (request == null)
            {
                response.SetFail("Request cannot be null.", EErrorCode.NullRequestExceptions);
                return;
            }

            var query = new SurveyHistoryQuery
            {
                ResultId = request.ResultId,
                UserId = request.UserId,
                SurveyId = request.SurveyId,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            var historyResponse = await surveyService.GetSurveyHistory(query);

            if (!historyResponse.Status || historyResponse.Data == null)
            {
                response.SetFail(historyResponse.ErrorMessages);
                return;
            }
            
            var historyContainer = historyResponse.Data;
            //historyContainer.ParseAndAssignData();

            response.Data = historyContainer.Data;
            response.SetSuccess();
        });
    }
}