using AccountServiceObject.QueryObjects;
using CommonGrpcObject;
using CommonWebApplication.Services;
using Enums;
using IQuestionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionServiceObject.BusinessObjects;
using QuestionServiceObject.QueryObjects;
using SSO.OutputModels;
using SSO.Requests;

namespace SSO.Controllers;

public class SurveyController(
    ILogger<AccountController> logger,
    ContextService contextService,
    ISurveyService surveyService
) : BaseSsoController(logger, contextService)
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<CommonApiResponse<object>> QuestionGetBySurvey([FromBody] QuestionGetBySurveyIdRequest? request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            // await Logout();
            if (request == null)
            {
                response.SetFail(EErrorCode.NullRequestExceptions);
                return;
            }

            var questionsBySurvey = await surveyService.GetQuestionsBySurvey(new SurveyGetAllQuery
            {
                SurveyIdAsString = request.SurveyId,
            });
            List<QuestionObject>? questions = questionsBySurvey.Data;
            if (!questionsBySurvey.Status)
            {
                response.SetFail(questionsBySurvey.ErrorMessages);
                return;
            }

            List<QuestionsBySurveyModel>? questionModels = questions?.Select(x => new QuestionsBySurveyModel()
            {
                QuestionText = x.QuestionText,
                QuestionType = x.QuestionType,
                QuestionOrder = x.QuestionOrder,
                IsRequired = x.IsRequired,
                MaxScore = x.MaxScore,
            }).ToList();
            
            response.Data = new
            {
                Models = questionsBySurvey.Data,
            };
            response.SetSuccess();
        });
    }
}