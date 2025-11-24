using IQuestionService;
using QuestionServiceObject.CommandObjects;
using QuestionServiceObject.QueryObjects;

namespace QuestionService.Services;

public class HostingApp(
    ILogger<HostingApp> logger,
    IFaqService faqService,
    ISurveyService surveyService
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
        
        var testSurveyAdd = (await surveyService.AddOrUpdateSurvey(new SurveyAddOrUpdateCommand()
        {
            Id = "5a5a6caf-db94-4d31-bb34-6fa2c9fcef73",
            Title = "Khảo sát mức độ hài lòng về dịch vụ khách hàng ccccccccc",
            Description = "Đây là một khảo sát ngắn nhằm thu thập ý kiến của khách hàng về chất lượng và trải nghiệm dịch vụ của chúng tôi trong quý 4 năm 2025.",
            IsPublished = true,
            ExpiredAt = DateTime.Now.AddDays(30),
        })).Data;
        
        var testSurveyAdd2 = (await surveyService.AddOrUpdateSurvey(new SurveyAddOrUpdateCommand()
        {
            Id = "5a5a6caf-db94-4d31-bb34-6fa2c9fcef73",
            Title = "Khảo sát mức độ hài lòng về dịch vụ khách hàng ccccccccc",
            Description = "Đây là một khảo sát ngắn nhằm thu thập ý kiến của khách hàng về chất lượng và trải nghiệm dịch vụ của chúng tôi trong quý 4 năm 2025.",
            IsPublished = true,
            ExpiredAt = DateTime.Now.AddDays(30),
        })).Data;
        
        var testSurveyAdd3 = (await surveyService.AddOrUpdateSurvey(new SurveyAddOrUpdateCommand()
        {
            Id = "5a5a6caf-db94-4d31-bb34-6fa2c9fcef73",
            Title = "Khảo sát mức độ hài lòng về dịch vụ khách hàng ccccccccc",
            Description = "Đây là một khảo sát ngắn nhằm thu thập ý kiến của khách hàng về chất lượng và trải nghiệm dịch vụ của chúng tôi trong quý 4 năm 2025.",
            IsPublished = true,
            ExpiredAt = DateTime.Now.AddDays(30),
        })).Data;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("AccountService AppHostedService is stopping");
        return Task.CompletedTask;
    }
}