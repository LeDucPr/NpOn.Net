using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonWebApplication.Services;
using DbFactory;
using IAccountService;

namespace AccountService.Services;

public class AuthenticationService(
IDbFactoryWrapper dbFactoryWrapper,
    ILogger<CommonService> logger
    ) : CommonService(logger), IAuthenticationService
{
    public async Task<CommonResponse<INpOnGrpcObject>> Login()
    {
        throw new NotImplementedException();
    }
}