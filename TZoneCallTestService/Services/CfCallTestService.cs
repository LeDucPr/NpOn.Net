using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonObject.CommonObjects;
using CommonWebApplication.Services;
using DbFactory;
using ITZoneCallTestService;
using ITZoneService;

namespace TZoneCallTestService.Services;

public class CfCallTestService(
    // IDbFactoryWrapper dbFactoryWrapper,
    ICfService cfService,
    ILogger<CommonService> logger
) : CommonService(logger), ICfCallTestService
{
    public async Task<CommonResponse<INpOnGrpcObject>> TestCallC()
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var accResponse = await cfService.TestC();
            INpOnGrpcObject? resultAcc = accResponse.Data;
            if (accResponse.Status && resultAcc != null)
            {
                response.Data = resultAcc;
                response.SetSuccess();
            }
        });
    }
}
