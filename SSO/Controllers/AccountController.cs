using AccountServiceObject.QueryObjects;
using CommonGrpcObject;
using CommonObject;
using CommonWebApplication.Attributes;
using CommonWebApplication.Services;
using IAccountService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEnums.AccountEnums;
using SSO.Mappings.Account;
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
            var validator = AccountLoginRequestValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            AccountLoginQuery inputQuery = new AccountLoginQuery
            {
                Email = request.Email,
                ClientId = contextService.ClientId,
                PhoneNumber = request.PhoneNumber,
                UserName = request.UserName,
                Password = request.Password,
                DeviceLoginInfo = request.DeviceInfo,
                LoginType = request.LoginType,
                Ip = contextService.GetIp(),
                AuthenApplicationId = request.AppId,
                AuthType = request.AuthType,
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
                Model = accountLoginResponse.Data.ToModel(),
            };
            response.SetSuccess();
        });
    }

    [HttpPost]
    public async Task<CommonApiResponse<object>> RefreshToken([FromBody] AccountRefreshTokenRequest request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            var validator = AccountRefreshTokenValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            AccountRefreshTokenQuery inputQuery = new AccountRefreshTokenQuery
            {
                RefreshToken = request.RefreshToken,
                DeviceInfo = request.DeviceInfo,
                LoginType = request.LoginType,
                AuthType = request.AuthType,
                SessionId = contextService.GetSessionKey().AsDefaultString(),
                ProcessUId = contextService.GetAccountIdAsString(),
            };
            var accountLoginResponse = await authenticationService.RefreshToken(inputQuery);
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
                Model = accountLoginResponse.Data.ToModel(),
            };
            response.SetSuccess();
        });
    }
    
    [PermissionController(EPermission.SuperUser, EPermission.Administrator)]
    [HttpPost]
    public async Task<CommonApiResponse<object>> CCCCCCC([FromBody] AccountRefreshTokenRequest request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            // response.Data = await LoginProcess(tokenResult.Data);
            response.Data = new
            {
                Model = "Oke phân quyền xong",
            };
            response.SetSuccess();
        });
    }
    
}