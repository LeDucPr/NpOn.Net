using Controllers.NpOn.SSO.OutputModels;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

namespace Controllers.NpOn.SSO.Mappings.Account;

public static class AccountModelMapping
{
    public static AccountLoginOutputModel ToModel(this AccountLoginRModel accountLogin)
    {
        return new AccountLoginOutputModel
        {
            AccountId = accountLogin.AccountId,
            AuthType = accountLogin.AuthType,
            LoginType = accountLogin.LoginType,
            FullName = accountLogin.FullName,
            PhoneNumber = accountLogin.PhoneNumber,
            Token = accountLogin.Token,
            RefreshToken = accountLogin.RefreshToken,
            CreatedAt = accountLogin.CreatedAt,
            SessionId = accountLogin.SessionId,
            MinuteExpire = accountLogin.MinuteExpire,
        };
    }
}