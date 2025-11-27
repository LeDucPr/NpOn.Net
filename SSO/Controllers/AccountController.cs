using AccountServiceObject.QueryObjects;
using CommonGrpcObject;
using CommonWebApplication.Services;
using Enums;
using IAccountService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSO.Requests;
using SSO.Validators;

namespace SSO.Controllers;

public class AccountController(
    ILogger<AccountController> logger,
    ContextService contextService,
    IAuthenticationService authenticationService
) : BaseSsoController(logger, contextService)
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<CommonApiResponse<object>> Login([FromBody] AccountLoginRequest request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            // var validator = AccountLoginRequestValidator.ValidateRequest(request);
            // if (!validator.IsValid)
            // {
            //     response.SetFail(validator.Errors.Select(p => p.ToString()));
            //     return;
            // }

            // type 1
            AccountLoginQuery inputQuery = new AccountLoginQuery
            {
                Email = request.Email,
                ClientId = contextService.ClientId,
                PhoneNumber = request.PhoneNumber,
                UserName = request.UserName,
                Password = request.Password,
                DeviceLoginInfo = request.DeviceInfo,
                LoginType =  request.LoginType,
                Ip = contextService.GetIp(),
                AuthenApplicationId = request.AppId,
            };
            var accountLoginResponse = await authenticationService.Login(inputQuery);
            if (!accountLoginResponse.Status)
            {
                response.SetFail(accountLoginResponse.ErrorMessages);
                return;
            }
            
            if (accountLoginResponse.Data == null || string.IsNullOrEmpty(accountLoginResponse.Data.Token))
            {
                response.SetFail("Login invalid");
                return;
            }
            
            // response.Data = await LoginProcess(tokenResult.Data);
            response.Data = new
            {
                Model = accountLoginResponse.Data,
            };
            response.SetSuccess();
        });
    }
}