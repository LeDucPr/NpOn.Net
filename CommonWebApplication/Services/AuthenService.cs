using AccountServiceObject.BusinessObjects;

namespace CommonWebApplication.Services;

public class AuthenService(ILogger<CommonService> logger) : CommonService(logger)
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
}