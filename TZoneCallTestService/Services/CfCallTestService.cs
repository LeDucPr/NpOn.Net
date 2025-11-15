using AccountServiceObject.BusinessObjects;
using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonObject.CommonObjects;
using CommonWebApplication.Services;
using DbFactory;
using IAccountService;
using ITZoneCallTestService;
using ITZoneService;

namespace TZoneCallTestService.Services;

public class CfCallTestService(
    // IDbFactoryWrapper dbFactoryWrapper,
    ICfService cfService,
    IUserService userService,
    ILogger<CommonService> logger
) : CommonService(logger), ICfCallTestService
{
    public async Task<CommonResponse<INpOnGrpcObject>> TestCallC()
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            // var accResponse = await cfService.TestC();

            var accResponse = await userService.GetAccountInfo();
            AccountInfoAliasTestObject? resultAcc = accResponse.Data;
            if (accResponse.Status || resultAcc != null)
            {
                Console.WriteLine(resultAcc.Host);
            }


            var ccGeObjResponse = await userService.GetAccountInfoAsGenericTable();
            INpOnGrpcObject? resultAccGeObj = ccGeObjResponse.Data;
            if (resultAccGeObj is not NpOnGrpcTable table || !ccGeObjResponse.Status)
            {
                response.SetFail("Sai CCCCCCCCCCC");
                return;
            }

            if (table.Rows is not { Count: > 0 })
            {
                response.SetFail("Empty table");
                return;
            }

            foreach (var row in table.Rows)
            {
                Console.WriteLine("---------------------------------------");
                foreach (var cell in row.Value.Cells)
                    Console.Write($"----   {cell.Value.ValueAsObject}   --");
                Console.WriteLine("---------------------------------------");
            }

            response.Data = resultAccGeObj;
            response.SetSuccess();
        });
    }
}