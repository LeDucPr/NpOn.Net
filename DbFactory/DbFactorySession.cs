using CommonMode;
using CommonObject;
using Enums;

namespace DbFactory;

public static class DbFactorySession
{
    private static readonly WrapperCacheStore<string, IDbFactoryWrapper> DbFactoryWrapperCache = new();

    private static void AddToDbFactoryWrapperCache(this IDbFactoryWrapper dbFactoryWrapper)
    {
        string? key = dbFactoryWrapper.FactoryOptionCode;
        if (string.IsNullOrWhiteSpace(key))
            throw new KeyNotFoundException(EDbError.ConnectOption.GetDisplayName());
        DbFactoryWrapperCache.AddOrUpdate(key, _ => dbFactoryWrapper, (_, existing) => existing);
    }

    private static IDbFactoryWrapper GetFactoryWrapperFromCache(string key)
    {
        // get from cache
        if (DbFactoryWrapperCache.TryGetValue(key, out var wrapper) && wrapper != null)
            return wrapper;
        // If not, get all keyCode of ConnectOption
        IDbFactoryWrapper? foundWrapper = DbFactoryWrapperCache.GetAll()
            .FirstOrDefault(kv => kv.Value.FactoryOptionCode == key).Value;

        if (foundWrapper != null)
            return foundWrapper;

        throw new KeyNotFoundException(EDbError.ConnectOption.GetDisplayName());
    }
}