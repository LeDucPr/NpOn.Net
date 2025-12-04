using CommonMode;
using CommonObject;
using Enums;

namespace DbFactory;

public static class DbFactorySessions // DbFactoryWrapperExtension
{
    private static readonly WrapperCacheStore<string, IDbFactoryWrapper> DbFactoryWrapperCache = new();

    #region Public

    public static List<string> GetAllKeyFromDbFactoryWrapperCache()
    {
        return DbFactoryWrapperCache.GetAll().Select(x => x.Key).ToList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">Equals key of ConnectOption</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static IDbFactoryWrapper? GetFactoryWrapperFromCache(string key)
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

    #endregion Public


    #region Internal

    internal static void AddToDbFactoryWrapperCache(this IDbFactoryWrapper dbFactoryWrapper)
    {
        string? key = dbFactoryWrapper.FactoryOptionCode;
        if (string.IsNullOrWhiteSpace(key))
            throw new KeyNotFoundException(EDbError.ConnectOption.GetDisplayName());
        DbFactoryWrapperCache.AddOrUpdate(key, _ => dbFactoryWrapper, (_, existing) => existing);
    }

    #endregion Internal
}