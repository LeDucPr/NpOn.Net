using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonWebApplication.Services;
using Enums;
using IQuestionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var httpRequest = this.HttpContext.Request;

            // // --- Lấy các thông tin cơ bản của Request ---
            // var method = httpRequest.Method; // Ví dụ: "POST"
            // var path = httpRequest.Path; // Ví dụ: "/api/Question/QuestionGetBySurvey"
            // var queryString = httpRequest.QueryString.ToString(); // Ví dụ: "?id=123"
            //
            // // --- Lấy thông tin Header ---
            // // Log tất cả headers
            // var headers = string.Join(", ", httpRequest.Headers.Select(h => $"{h.Key}: {h.Value}"));
            //
            // // Log thông tin đã lấy được
            // Console.WriteLine($"\n--- Request Details ---");
            // Console.WriteLine($"Method: {method}");
            // Console.WriteLine($"Path: {path}");
            // Console.WriteLine($"Query String: {queryString}");
            // Console.WriteLine($"Headers: {headers}");
            // Console.WriteLine($"Body (đã deserialize): {System.Text.Json.JsonSerializer.Serialize(request)}"); 
            // // ...
            
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
                Models = outputModel, ////////// ????????????????
            };
            response.SetSuccess();
        });
    }
}