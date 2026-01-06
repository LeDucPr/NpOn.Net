using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.DbFactory.Redis;
using Definitions.NpOn.ProjectConstant.AccountConstant;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;

namespace MicroServices.Account.StorageAdapter.NpOn.AccountStorageAdapter;

public class AccountTokenAndPermissionRedisRepository
    : IAccountTokenAndPermissionRedisRepository
{
    private readonly IRedisFactoryWrapper _redisCachingFactoryWrapper;
    private readonly int _expireTokenMinutes;
    private readonly int _expireTokenStorageMinutes;

    public AccountTokenAndPermissionRedisRepository(IRedisFactoryWrapper redisCachingFactoryWrapper)
    {
        _redisCachingFactoryWrapper = redisCachingFactoryWrapper;
        _expireTokenMinutes =
            EApplicationConfiguration.LoginExpiresTime.GetAppSettingConfig().AsDefaultInt() != 0
                ? EApplicationConfiguration.LoginExpiresTime.GetAppSettingConfig().AsDefaultInt()
                : 480;
        _expireTokenStorageMinutes = _expireTokenMinutes + 1;
    }


    #region Token & Storage

    public async Task AddCachingToken(string sessionId, AccountLoginRModel accountLogin)
    {
        await _redisCachingFactoryWrapper.SetAsync($"{AccountCachingCode.PrefixCachingAccountToken}{sessionId}",
            JsonMode.ToJson(accountLogin),
            TimeSpan.FromMinutes(_expireTokenMinutes));
    }

    public async Task<AccountLoginRModel?> GetAccountLogonBySessionIdWithPrefixCode(string sessionId)
    {
        sessionId = $"{AccountCachingCode.PrefixCachingAccountToken}{sessionId}";
        var accountLoginInfoCache =
            await _redisCachingFactoryWrapper.GetStringAsync(sessionId);
        if (accountLoginInfoCache == null)
            return null;
        var cacheValue = accountLoginInfoCache.Result.Values.FirstOrDefault()?.ValueAsObject.AsEmptyString();
        if (string.IsNullOrEmpty(cacheValue))
            return null;
        if (JsonMode.TryFromJson<AccountLoginRModel>(cacheValue, out var accountLoginInfoRModel))
            if (accountLoginInfoRModel != null)
                return accountLoginInfoRModel;
        return null;
    }

    public async Task DeleteCachingToken(string sessionId)
    {
        sessionId = $"{AccountCachingCode.PrefixCachingAccountToken}{sessionId}";
        await _redisCachingFactoryWrapper.DeleteAsync(sessionId);
    }


    public async Task<bool> AddToCachingTokenStorageByAccountId(string accountId, string sessionId)
    {
        string accountCachingTokenStorage = $"{AccountCachingCode.PrefixCachingAccountTokenStorage}{accountId}";
        string[] existedAccountCachingTokenStorages = [$"{AccountCachingCode.PrefixCachingAccountToken}{sessionId}"];
        existedAccountCachingTokenStorages = existedAccountCachingTokenStorages
            .Concat(await GetCachingTokenStorageByAccountId(accountId) ?? []).ToArray();
        await _redisCachingFactoryWrapper.SetAsync(accountCachingTokenStorage,
            JsonMode.ToJson(existedAccountCachingTokenStorages),
            TimeSpan.FromMinutes(_expireTokenStorageMinutes));
        return true;
    }

    public async Task<bool> DeleteCachingTokenStorageAndTokensByAccountId(string accountId, string[]? sessionIds = null)
    {
        string accountCachingTokenStorage = $"{AccountCachingCode.PrefixCachingAccountTokenStorage}{accountId}";
        string[]? existedAccountCachingTokenStorages = await GetCachingTokenStorageByAccountId(accountId);
        if (existedAccountCachingTokenStorages == null)
            return true;
        // sessionIds => delete all
        sessionIds = sessionIds?.Select(x => $"{AccountCachingCode.PrefixCachingAccountToken}{x}").ToArray()
                     ?? existedAccountCachingTokenStorages;
        if (sessionIds.Length == existedAccountCachingTokenStorages.Length)
            await _redisCachingFactoryWrapper.DeleteManyAsync(
                sessionIds.Concat([accountCachingTokenStorage]).ToArray());
        else
            await _redisCachingFactoryWrapper.DeleteManyAsync(sessionIds);

        // Update the storage list by removing the deleted session IDs.
        var remainingSessionIds = existedAccountCachingTokenStorages.Where(x => !sessionIds.Contains(x)).ToArray();
        if (remainingSessionIds is { Length: > 0 })
            await _redisCachingFactoryWrapper.SetAsync(accountCachingTokenStorage,
                JsonMode.ToJson(remainingSessionIds),
                TimeSpan.FromMinutes(_expireTokenStorageMinutes));
        return true;
    }

    public async Task<bool> DeleteCachingTokenStorageAndTokensByAccountIds(string[] accountIds)
    {
        string[] accountCachingTokenStorages = accountIds
            .Select(accountId => $"{AccountCachingCode.PrefixCachingAccountTokenStorage}{accountIds}").ToArray();
        string[]? existedAccountCachingTokenStorages = await GetCachingTokenStorageByAccountIds(accountIds);
        if (existedAccountCachingTokenStorages == null)
            return true;
        if (accountCachingTokenStorages.Length > 0)
            await _redisCachingFactoryWrapper.DeleteManyAsync(
                accountCachingTokenStorages.Concat(existedAccountCachingTokenStorages).ToArray());
        return true;
    }

    private async Task<string[]?> GetCachingTokenStorageByAccountId(string accountId)
    {
        string accountCachingTokenStorage = $"{AccountCachingCode.PrefixCachingAccountTokenStorage}{accountId}";
        var accountCachingTokenStorageOld =
            await _redisCachingFactoryWrapper.GetStringAsync(accountCachingTokenStorage);
        if (accountCachingTokenStorageOld != null)
        {
            var cacheValue = accountCachingTokenStorageOld.Result.Values.FirstOrDefault()?.ValueAsObject
                .AsEmptyString();
            if (JsonMode.TryFromJson<string[]>(cacheValue, out string[]? accountLoginSessionIds))
                return accountLoginSessionIds;
        }

        return null;
    }

    private async Task<string[]?> GetCachingTokenStorageByAccountIds(string[] accountIds)
    {
        string[] accountCachingTokenStorages = accountIds
            .Select(accountId => $"{AccountCachingCode.PrefixCachingAccountTokenStorage}{accountId}").ToArray();
        var accountCachingTokenStorageOld =
            await _redisCachingFactoryWrapper.GetManyStringAsync(accountCachingTokenStorages);
        if (accountCachingTokenStorageOld != null)
        {
            var cacheValue = accountCachingTokenStorageOld.Result.Values.FirstOrDefault()?.ValueAsObject
                .AsEmptyString();
            if (JsonMode.TryFromJson<string[]>(cacheValue, out string[]? accountLoginSessionIds))
                return accountLoginSessionIds;
        }

        return null;
    }

    #endregion Token & Storage


    #region Permission

    public async Task<bool> AddCachingPermissionException(string accountId,
        AccountPermissionExceptionRModel[]? accountPermissions)
    {
        if (accountPermissions != null)
            if (!await AddCachingPermissionException(
                    $"{AccountCachingCode.PrefixCachingAccountPermissionException}{accountId}",
                    accountPermissions.Select(x => x.ControllerCode).ToArray()))
            {
                return false;
            }

        return true;
    }

    private async Task<bool> AddCachingPermissionException(string cachingExceptionPermissionKey,
        string[] permissionExceptionControllerCodes)
    {
        if (!(await _redisCachingFactoryWrapper.SetAsync(cachingExceptionPermissionKey,
                JsonMode.ToJson(permissionExceptionControllerCodes),
                TimeSpan.FromMinutes(_expireTokenMinutes)))?.Status ?? false)
            return false;
        return true;
    }


    public async Task<string[]?> GetCachingPermissionException(string accountId)
    {
        var permissionExceptionCache =
            await _redisCachingFactoryWrapper.GetStringAsync(
                $"{AccountCachingCode.PrefixCachingAccountPermissionException}{accountId}");
        if (permissionExceptionCache == null) return null;

        var cacheValue = permissionExceptionCache.Result.Values.FirstOrDefault()?.ValueAsObject.AsEmptyString();
        if (string.IsNullOrEmpty(cacheValue)) return null;

        if (JsonMode.TryFromJson<string[]>(cacheValue, out var permissionExceptionControllerCodes))
            return permissionExceptionControllerCodes;
        return null;
    }

    // do not delete from redis (many session run on will be make error 403 forbidden)
    public async Task<bool> DeleteCachingPermissionExceptionsByAccountId(string accountId)
    {
        string cacheKey = $"{AccountCachingCode.PrefixCachingAccountPermissionException}{accountId}";
        await _redisCachingFactoryWrapper.DeleteManyAsync(cacheKey);
        return true;
    }

    public async Task<bool> DeleteCachingPermissionExceptionsByAccountIds(string[] accountIds)
    {
        string[] cacheKeys = accountIds
            .Select(accountId => $"{AccountCachingCode.PrefixCachingAccountPermissionException}{accountId}").ToArray();
        await _redisCachingFactoryWrapper.DeleteManyAsync(cacheKeys);
        return true;
    }

    #endregion Permission
}