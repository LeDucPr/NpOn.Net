using CommonDb.DbResults;
using CommonObject.CommonObjects;
using CommonWebApplication.Services;
using DbFactory;
using ITZoneService;

namespace TZoneService.Services;

public class CfService(
    IDbFactoryWrapper dbFactoryWrapper,
    ILogger<CommonService> logger
) : CommonService(logger), ICfService
{
    public async Task<CommonResponse<INpOnWrapperResult>> TestC()
    {
        return await CommonProcess<INpOnWrapperResult>(async (response) =>
        {
            string msQuery = "Select * from Users where id = 'C000175'";
            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.QueryAsync(msQuery);

            if (resultOfQuery != null)
            {
                response.Data = resultOfQuery;
                response.SetSuccess();
            }
        });
    }
}
