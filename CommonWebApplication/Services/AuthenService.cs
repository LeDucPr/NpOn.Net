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
    public AccountLoginInfoObject? GetLoginInfoSync(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        // Redis 
        // key = GetKey(key);
        // RedisValue value = WriteDatabase().StringGet(key);
        // return ConvertOutput<AccountLoginInfo>(value);

        // TODO:
        // Chưa có redis lưu tạm Db thường 

        return null;
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