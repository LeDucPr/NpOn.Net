using AccountServiceObject.BusinessObjects;
using ProjectEnums.AccountEnums;
using SSO.OutputModels;

namespace SSO.Mappings.Account;

public static class AccountModelMapping
{
    public static AccountLoginOutputModel ToModel(this AccountLoginInfoObject accountLoginInfo)
    {
        return new AccountLoginOutputModel
        {
            AccountId = accountLoginInfo.AccountId,
            AuthType = accountLoginInfo.AuthType,
            LoginType = accountLoginInfo.LoginType,
            FullName = accountLoginInfo.FullName,
            PhoneNumber = accountLoginInfo.PhoneNumber,
            Token = accountLoginInfo.Token,
            RefreshToken = accountLoginInfo.RefreshToken,
            CreatedAt = accountLoginInfo.CreatedAt,
            SessionId = accountLoginInfo.SessionId,
            MinuteExpire = accountLoginInfo.MinuteExpire,
        };
    }
}