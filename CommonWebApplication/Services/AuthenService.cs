using AccountServiceObject.BusinessObjects;
using AccountServiceObject.QueryObjects;
using CommonObject;
using IAccountService;

namespace CommonWebApplication.Services;

public class AuthenService(
    ILogger<CommonService> logger,
    IAuthenticationService authenticationService
) : CommonService(logger)
{
    /// <param name="key">sessionId</param>
    /// <returns></returns>
    public AccountLoginInfoObject? GetLoginInfoSync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;
        var loginInfo = GetLogonInfoBySessionId(key).GetAwaiter().GetResult();
        return loginInfo;
    }

    public async Task<AccountLoginInfoObject?> GetLogonInfoBySessionId(string sessionId)
    {
        var logonResponse = await authenticationService.GetLogonTokenBySessionId(new AccountGetLogonInfoBySessionIdQuery
        {
            SessionId = sessionId.AsDefaultString(),
        });
        if (!logonResponse.Status || logonResponse.Data == null)
            return null;
        return logonResponse.Data;
    }
}