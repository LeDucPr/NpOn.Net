using System.Security.Cryptography;
using AccountServiceObject.CommandObjects;
using AccountServiceObject.QueryObjects;
using CommonGrpcObject;
using CommonObject;
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
    [Obsolete("Obsolete")]
    [AllowAnonymous]
    [HttpPost]
    public async Task<CommonApiResponse<object>> SignIn([FromBody] AccountSigninRequest request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            var validator = AccountSigninRequestValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            var signinResponse = await authenticationService.Signin(new AccountSigninCommand
            {
                AuthType = request.AuthType,
                ClientId = contextService.ClientId,
                Email = request.Email.AsEmptyString(),
                PhoneNumber = request.PhoneNumber.AsEmptyString(),
                UserName = request.UserName.AsEmptyString(),
                Password = CreateHashPassword(request.Password)
                    .AsEmptyString(),
                LoginType = ELoginType.Default,
                SigninIp = contextService.GetIp(),
                DeviceSigninInfo = request.DeviceInfo,
                AuthenApplicationId = request.AppId,
                FullName = request.FullName.AsEmptyString(),
            });

            if (!signinResponse.Status)
            {
                string errMessages = signinResponse.ErrorMessages.AsArrayJoin();
                response.SetFail(!string.IsNullOrWhiteSpace(errMessages) ? errMessages : "Signin fail");
                return;
            }
            response.Data = new
            {
                Model = signinResponse.Data?.ToModel(),
            };
            response.SetSuccess();
        });
    }

    [Obsolete("Obsolete")]
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

            bool isUseMultiDevice = !string.IsNullOrWhiteSpace(request.DeviceInfo) &&
                                    request.AuthType == EAuthentication.WebApp;
            AccountLoginQuery inputQuery = new AccountLoginQuery
            {
                Email = request.Email,
                ClientId = contextService.ClientId,
                PhoneNumber = request.PhoneNumber,
                UserName = request.UserName,
                Password = CreateHashPassword(request.Password),
                DeviceLoginInfo = request.DeviceInfo,
                LoginType = request.LoginType,
                Ip = contextService.GetIp(),
                AuthenApplicationId = request.AppId,
                AuthType = request.AuthType,
                IsEnableMultiDevice = isUseMultiDevice
            };
            var accountLoginResponse = await authenticationService.Login(inputQuery);
            if (!accountLoginResponse.Status)
            {
                response.SetFail(accountLoginResponse.ErrorMessages);
                return;
            }

            if (accountLoginResponse.Data == null || string.IsNullOrEmpty(accountLoginResponse.Data.Token))
            {
                response.SetFail("Login fail");
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

    [HttpPost]
    public async Task<CommonApiResponse<string>> Logout([FromBody] AccountLogoutRequest request)
    {
        return await ProcessRequest<string>(async (response) =>
        {
            var logoutResponse = await authenticationService.LogOut(
                new AccountLogoutQuery
                {
                    SessionId = contextService.GetSessionKey().AsDefaultString(),
                    ProcessUId = contextService.GetAccountIdAsString(),
                });

            response.Data = logoutResponse.Status ? "Logout successful" : "Logout fail";
            if (!logoutResponse.Status)
            {
                response.SetFail(logoutResponse.ErrorMessages);
                return;
            }

            response.SetSuccess();
        });
    }

    #region private func
    
    [Obsolete("Obsolete")]
    private string CreateHashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return string.Empty;
        byte[] salt = System.Text.Encoding.UTF8.GetBytes(ContextService.DefaultSaltPassword);
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 369);
        byte[] hash = pbkdf2.GetBytes(20); // 160 bit
        return Convert.ToBase64String(hash);
    }

    #endregion private func
}