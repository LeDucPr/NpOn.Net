using AccountServiceObject;
using AccountServiceObject.QueryObjects;
using CommonGrpcObject;
using CommonWebApplication.Controllers;
using CommonWebApplication.Services;
using Enums;
using IAccountService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace SSO.Controllers;

public class AccountController(
    ILogger<AccountController> logger,
    ContextService contextService,
    IAuthenticationService authenticationService
) : BaseSsoController(logger, contextService)
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<CommonApiResponse<object>> Login([FromBody] LoginRequest? request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            // await Logout();
            if (request == null)
            {
                response.SetFail(EErrorCode.NullRequestExceptions);
                return;
            }

            // type 1
            AccountLoginQuery inputQuery = new AccountLoginQuery()
            {
                Email = request.Email,
            };
            var tokenResult = await authenticationService.Login(inputQuery);
            if (!tokenResult.Status)
            {
                response.SetFail(tokenResult.ErrorMessages);
                return;
            }
            
            
            // type 2
            string jsonInputQuery = CommonObject.JsonConverter.ToJson(inputQuery);

            var tokenResult2 = await authenticationService.LoginJ(new CommonJsonQuery
            {
                Json = jsonInputQuery,
            });
            if (!tokenResult2.Status)
            {
                response.SetFail(tokenResult.ErrorMessages);
                return;
            }

            // response.Data = await LoginProcess(tokenResult.Data);
            response.Data = "84780rhf89h289rh2";
            response.SetSuccess();
        });
    }
}