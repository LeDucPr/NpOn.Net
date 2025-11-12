using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
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
    public async Task<CommonResponse<INpOnGrpcObject>> TestC()
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            string pgQuery = "Select * from server_ctrl";
            pgQuery = $"SELECT _id FROM data_t_ ORDER BY _id DESC LIMIT {1}";
            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.QueryAsync(pgQuery);
            
            if (!(resultOfQuery is INpOnTableWrapper tableWrapper))
            {
                response.SetFail(["Incorrect data type"]);
                return;
            }

            var grpcTable = tableWrapper.ToGrpcTable();

            response.Data = grpcTable;
            response.SetSuccess();
        });
    }
}