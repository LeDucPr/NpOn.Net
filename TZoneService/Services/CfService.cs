using CommonDb.DbResults;
using CommonWebApplication.Services;
using DbFactory;
using ITZoneService;

namespace TZoneService.Services;

public class CfService(
    IDbFactoryWrapper dbFactoryWrapper,
    ILogger<CommonService> logger
) : CommonService(logger), ICfService
{
    public async Task<string> CCC()
    {
        string msQuery = "Select * from Users where id = 'C000175'";
        INpOnWrapperResult? resultOfQuery = dbFactoryWrapper?.QueryAsync(msQuery).GetAwaiter().GetResult();
        return "CCCCCCCCCCCCCC";
    }
}