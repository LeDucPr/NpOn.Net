using CommonDb.DbResults;
using DbFactory.Generics;
using RedisExtCm.Results;

namespace DbFactory.Redis;

public interface IRedisFactoryWrapper : IDbFactoryWrapper
{
    #region Single Operations

    Task<INpOnWrapperResult?> GetAsync(string key);
    Task<INpOnWrapperResult?> SetAsync(string key, string value);
    Task<INpOnWrapperResult?> DeleteAsync(string key);
    Task<RedisValueWrapper?> GetStringAsync(string key);
    Task<RedisValueWrapper?> SetStringAsync(string key, string value);
    Task<RedisValueWrapper?> DeleteKeyAsync(string key);

    #endregion Single Operations


    #region Bulk Operations

    Task<INpOnWrapperResult?> GetManyAsync(params string[] keys);
    Task<INpOnWrapperResult?> SetManyAsync(Dictionary<string, string> keyValues);
    Task<INpOnWrapperResult?> DeleteManyAsync(params string[] keys);
    Task<RedisValueWrapper?> GetManyStringAsync(params string[] keys);
    Task<RedisValueWrapper?> SetManyStringAsync(Dictionary<string, string> keyValues);
    Task<RedisValueWrapper?> DeleteManyStringAsync(params string[] keys);

    #endregion Bulk Operations
}