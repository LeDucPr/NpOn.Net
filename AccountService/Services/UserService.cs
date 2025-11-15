using AccountServiceObject.BusinessObjects;
using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonWebApplication.Services;
using DbFactory;
using HandleFlow.ResultConverters;
using IAccountService;
using ObjectHandlerFlow.AlgObjs.CtrlObjs;

namespace AccountService.Services;

public class UserService(
    IDbFactoryWrapper dbFactoryWrapper,
    ILogger<CommonService> logger
) : CommonService(logger), IUserService
{
    public async Task<CommonResponse<AccountInfoAliasTestObject>> GetAccountInfo()
    {
        return await CommonProcess<AccountInfoAliasTestObject>(async (response) =>
        {
            string pgQuery = "SELECT * FROM server_ctrl";
        
            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.QueryAsync(pgQuery);
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
            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.QueryAsync(pgQuery);
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