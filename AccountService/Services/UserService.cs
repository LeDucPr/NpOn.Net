using AccountServiceObject;
using AccountServiceObject.BusinessObjects;
using CommonDb.DbCommands;
using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonWebApplication.Services;
using DbFactory;
using DbFactory.Generics;
using HandleFlow.ResultConverters;
using IAccountService;
using NpgsqlTypes;

namespace AccountService.Services;

public class UserService(
    IDbFactoryWrapper dbFactoryWrapper,
    ILogger<CommonService> logger
) : CommonService(logger), IUserService
{
    public async Task<CommonResponse<BaseAccountExecFuncJsonObject?>> GetAccountInfos()
    {
        return await CommonProcess<BaseAccountExecFuncJsonObject?>(async (response) =>
        {
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
                      ""pageSize"": 1
                    }"
                }, true, isUseOutputJsonAsName: funcName
            );

            if (!(resultOfQuery is INpOnTableWrapper tableWrapper))
            {
                response.SetFail(["Incorrect data type"]);
                return;
            }

            List<BaseAccountExecFuncJsonObject>? accountObjects = resultOfQuery
                .GenericConverterForBaseAccountJson(typeof(BaseAccountExecFuncJsonObject), jsonColumnName: funcName)?
                .Cast<BaseAccountExecFuncJsonObject>()
                .ToList();
            accountObjects?.ForEach(x => x.ToObject<BaseAccountExecFuncJsonObject>());
            response.Data = accountObjects?.FirstOrDefault();
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<AccountInfoAliasTestObject>> GetAccountInfo()
    {
        return await CommonProcess<AccountInfoAliasTestObject>(async (response) =>
        {
            string pgQuery = "SELECT * FROM patient where patient_id = @patient_id";

            List<NpOnDbCommandParam> param =
            [
                new NpOnDbCommandParam<NpgsqlDbType>()
                {
                    ParamName = "patient_id",
                    ParamValue = "15132bb5-81cf-4567-992b-59e80f6f316d",
                    ParamType = NpgsqlDbType.Uuid,
                }
            ];
            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.ExecuteAsync(pgQuery, param);
            List<AccountInfoAliasTestObject>? accountObjects = resultOfQuery?
                .GenericConverter(typeof(AccountInfoAliasTestObject))?
                .Cast<AccountInfoAliasTestObject>()
                .ToList();

            if (accountObjects is not { Count: > 0 })
            {
                response.SetFail("Incorrect data type of 'IEnumerable<AccountInfoAliasTestObject>'");
                return;
            }

            AccountInfoAliasTestObject accountObject = accountObjects.First();

            response.Data = accountObject;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<INpOnGrpcObject>> GetAccountInfoAsGenericTable()
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            string pgQuery = "Select * from server_ctrl";
            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.ExecuteAsync(pgQuery);
            var ctrl = resultOfQuery?.GenericConverter(typeof(AccountLoginInfoObject));

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