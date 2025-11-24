using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonWebApplication.Services;
using Enums;
using IGeneralService;
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

            var surveyGetBy = await surveyService.GetQuestionsBySurveyId(new SurveyGetAllQuery()
            {
                SurveyIdAsString = request.SurveyId
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
                Models = outputModel, ////////// ????????????????
            };
            response.SetSuccess();
        });
    }
}