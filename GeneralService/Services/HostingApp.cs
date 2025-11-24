using GeneralServiceObject.QueryObjects;
using IGeneralService;

namespace GeneralService.Services;

public class HostingApp(
    ILogger<HostingApp> logger,
    IFldMasterPgService fldMasterPgService
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("GeneralService AppHostedService is starting");
        // var surveyGetBy = await fldMasterPgService.Query(new TblFldQuery()
        // {
        //     Code = "questions_by_survey_id", 
        //     QueryParams = [
        //         new TblFldQueryParam()
        //         {
        //             ParamName = "survey_id", 
        //             StringValue = "58555125-3746-4f94-9330-f84480094327"
        //         }
        //     ],
        // });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("GeneralService AppHostedService is stopping");
        return Task.CompletedTask;
    }
}