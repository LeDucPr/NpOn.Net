using CommonDb.DbResults;
using CommonObject.CommonObjects;
using CommonWebApplication.Services;
using DbFactory;
using ITZoneCallTestService;
using ITZoneService;

namespace TZoneCallTestService.Services;

public class CfCallTestService(
    IDbFactoryWrapper dbFactoryWrapper,
    ICfService cfService,
    ILogger<CommonService> logger
) : CommonService(logger), ICfCallTestService
{
    public async Task<CommonResponse<INpOnWrapperResult>> TestCallC()
    {
        return await CommonProcess<INpOnWrapperResult>(async (response) =>
        {
            var accResponse = await cfService.TestC();
            INpOnWrapperResult? resultAcc = accResponse.Data;
            if (accResponse.Status && resultAcc != null)
            {
                response.Data = resultAcc;
                response.SetSuccess();
            }
        });
    }
}
