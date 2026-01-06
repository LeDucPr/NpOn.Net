using Common.Extensions.NpOn.CommonMode;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using MicroServices.Account.Service.NpOn.IAccountService;

namespace Common.Extensions.NpOn.CommonWebApplication.Services;

public class AuthenService(
    ILogger<CommonService> logger,
    IAuthenticationService authenticationService
) : CommonService(logger)
{
    /// <param name="key">sessionId</param>
    /// <returns></returns>
    public AccountLoginRModel? GetLoginInfoSync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;
        var loginInfo = GetLogonInfoBySessionId(key).GetAwaiter().GetResult();
        return loginInfo;
    }

    public async Task<AccountLoginRModel?> GetLogonInfoBySessionId(string sessionId)
    {
        var logonResponse = await authenticationService.GetLogonTokenBySessionId(new AccountGetLogonInfoBySessionIdQuery
        {
            SessionId = sessionId.AsDefaultString(),
        });
        if (!logonResponse.Status || logonResponse.Data == null)
            return null;
        return logonResponse.Data;
    }

    public async Task<bool> CheckLogonPermissionExceptionControllers
        (string accountId, string controllerCode, bool isHasBasePermission)
    {
        var logonPermissionExceptionControllers =
            await authenticationService.IsValidLogonPermissionExceptionControllers(
                new AccountPermissionExceptionCachingCheckValidQuery
                {
                    AccountId = accountId.AsDefaultString(),
                    ControllerCode = controllerCode,
                    IsHasBasePermission = isHasBasePermission,
                });
        return logonPermissionExceptionControllers.Status & logonPermissionExceptionControllers.Data;
    }
}