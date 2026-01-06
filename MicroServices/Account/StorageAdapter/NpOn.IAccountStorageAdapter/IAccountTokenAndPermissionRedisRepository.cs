using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

namespace MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;

public interface IAccountTokenAndPermissionRedisRepository
{
    Task AddCachingToken(string sessionId, AccountLoginRModel accountLogin);
    Task<AccountLoginRModel?> GetAccountLogonBySessionIdWithPrefixCode(string sessionId);

    Task DeleteCachingToken(string sessionId);


    Task<bool> AddToCachingTokenStorageByAccountId(string accountId, string sessionId);
    Task<bool> DeleteCachingTokenStorageAndTokensByAccountId(string accountId, string[]? sessionIds = null);
    Task<bool> DeleteCachingTokenStorageAndTokensByAccountIds(string[] accountIds);
    
    Task<bool> AddCachingPermissionException(string accountId,
        AccountPermissionExceptionRModel[]? accountPermissions);

    Task<string[]?> GetCachingPermissionException(string accountId);

    // do not delete from redis (many session run on will be make error 403 forbidden)
    Task<bool> DeleteCachingPermissionExceptionsByAccountId(string accountId);
    Task<bool> DeleteCachingPermissionExceptionsByAccountIds(string[] accountIds);
    // Task AddCachingMenu(string sessionId, AccountLoginRModel accountLogin);
}