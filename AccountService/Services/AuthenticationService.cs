using AccountServiceObject.BusinessObjects;
using AccountServiceObject.QueryObjects;
using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonWebApplication.Services;
using DbFactory;
using HandleFlow.ResultConverters;
using IAccountService;

namespace AccountService.Services;

public class AuthenticationService(
    IDbFactoryWrapper dbFactoryWrapper,
    ILogger<CommonService> logger
) : CommonService(logger), IAuthenticationService
{
    public async Task<CommonResponse<AccountInfoAliasTestObject>> Login(AccountLoginQuery query)
    {
        return await CommonProcess<AccountInfoAliasTestObject>(async (response) =>
        {
            string? email = query.Email; // --------
            
            string pgQuery = "SELECT * FROM server_ctrl";

            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.ExecuteAsync(pgQuery);
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
            string? userName = accountObject.UserName;

            if (userName is not { Length: > 0 })
            {
                response.SetFail("Invalid username");
                return;
            }
            response.Data = accountObject;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<AccountInfoAliasTestObject>> LoginJ(CommonJsonQuery query)
    {
        return await CommonProcess<AccountInfoAliasTestObject>(async (response) =>
        {
            AccountLoginQuery? accountLogin = CommonObject.JsonConverter.FromJson<AccountLoginQuery?>(query.Json);
            // catch Json convert failure
            if (accountLogin == null)
            {
                response.SetFail("CommonJsonQuery -> AccountLoginQuery convert null");
                return;
            }
            response.Data = (await Login(accountLogin)).Data;
            response.SetSuccess();
        });
    }

    public Task<CommonResponse<INpOnGrpcObject>> RefreshToken(CommonJsonQuery query)
    {
        throw new NotImplementedException();
    }

    public Task<CommonResponse<INpOnGrpcObject>> LoginToken(CommonJsonQuery query)
    {
        throw new NotImplementedException();
    }

    public Task<CommonResponse<INpOnGrpcObject>> Info()
    {
        throw new NotImplementedException();
    }

    public Task<CommonResponse> LogOut()
    {
        throw new NotImplementedException();
    }
}