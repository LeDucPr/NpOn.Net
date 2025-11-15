using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
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
            // string pgQuery = "Select * from server_ctrl";
            // pgQuery = $"SELECT _id FROM data_t_ ORDER BY _id DESC LIMIT {1}";
            // INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.QueryAsync(pgQuery);

            string funcName = "sp_dyn_patient_rank_search_cccccccccccccccccccccc";

            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.ExecuteFunc(
                funcName,
                new Dictionary<string, object>
                {
                    [""] =
                        @"{
                      ""full_name"": """",
                      ""username"": """",
                      ""from_date"": ""2025-11-07T00:00:00"",
                      ""to_date"": ""2025-11-14T23:59:59"",
                      ""mobile_phone"": """",
                      ""gender"": """",
                      ""province_rcd"": """",
                      ""district_rcd"": """",
                      ""commune_rcd"": """",
                      ""standard_account_id"": ""12fbd6a7-978b-4e7f-98bc-43c21684b371"",
                      ""master_account_id"": null,
                      ""province_account_rcd"": """",
                      ""rank_type"": null,
                      ""page"": 1,
                      ""pageSize"": 50
                    }"
                }, true, isUseOutputJsonAsName: "object"
            );

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