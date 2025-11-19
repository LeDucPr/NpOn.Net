using IQuestionService;
using QuestionServiceObject.QueryObjects;

namespace QuestionService.Services;

public class HostingApp(
    ILogger<HostingApp> logger,
    IFaqService faqService,
    IQuestionService.ISurveyService surveyService
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("AccountService AppHostedService is starting");
        //var aaaa = (await faqService.GetAll(new FaqQuery()
        var test = (await surveyService.GetAllSurveys()).Data;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("AccountService AppHostedService is stopping");
        return Task.CompletedTask;
    }
}