using IQuestionService;
using QuestionServiceObject.CommandObjects;
using QuestionServiceObject.QueryObjects;

namespace QuestionService.Services;

public class HostingApp(
    ILogger<HostingApp> logger,
    IFaqService faqService,
    ISurveyService surveyService, 
    IQuestionAndAnswerService questionAndAnswerService
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("AccountService AppHostedService is starting");
        //var aaaa = (await faqService.GetAll(new FaqQuery()
        // var test = (await surveyService.GetAllSurveys()).Data;
        // var test = (await surveyService.GetQuestionsBySurvey(
        //     new SurveyGetAllQuery()
        //     {
        //         SurveyIdAsString = "'58555125-3746-4f94-9330-f84480094327'"
        //     })).Data;
        
        // var testCC = (await surveyService.GetQuestionsByUserIdAndSurveyId(new QuestionGetByUserIdAndSurveyIdQuery
        // {
        //     SurveyId = string.Empty,
        //     UserId = string.Empty,
        // })).Data;
        
        // var testSurveyAdd = (await surveyService.AddOrUpdateSurvey(new SurveyAddOrUpdateCommand()
        // {
        //     Id = "5a5a6caf-db94-4d31-bb34-6fa2c9fcef73",
        //     Title = "Khảo sát mức độ hài lòng về dịch vụ khách hàng",
        //     Description = "Đây là một khảo sát ngắn nhằm thu thập ý kiến của khách hàng về chất lượng và trải nghiệm dịch vụ của chúng tôi trong quý 4 năm 2025.",
        //     IsPublished = true,
        //     ExpiredAt = DateTime.Now.AddDays(30),
        // })).Data;
        
        // var testSubmit = (await questionAndAnswerService.SubmitAnswers(new SubmitSurveyCommand
        // {
        //     UserId = "61bf3d62-cc1d-49ca-b5bc-e19634e9b0fa",
        //     SurveyId = "58555125-3746-4f94-9330-f84480094327",
        //     Answers = [
        //         new SubmissionAnswer
        //         {
        //             QuestionId = "14962e44-a23a-43dc-b58a-376ba277c1ea",
        //             AnswerIds = ["bc855526-100f-4612-b906-130886ad8a8b", "13a8b713-2b46-4c41-9148-e98f5749a850"],
        //         }
        //     ],
        // })).Data;

        var calScoreTest = (await surveyService.CalculateScore(new CalculateSurveyScoreQuery()
        {
            UserId = "61bf3d62-cc1d-49ca-b5bc-e19634e9b0fa", 
            SurveyId = "58555125-3746-4f94-9330-f84480094327",
        })).Data;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("AccountService AppHostedService is stopping");
        return Task.CompletedTask;
    }
}