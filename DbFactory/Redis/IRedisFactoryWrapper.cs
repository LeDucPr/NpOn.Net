using DbFactory.Generics;
using RedisExtCm.Results;

namespace DbFactory.Redis;

public interface IRedisFactoryWrapper : IDbFactoryWrapper
{
    Task<RedisValueWrapper> GetStringAsync(string key);
    Task<RedisValueWrapper> SetStringAsync(string key, string value);
    Task<RedisValueWrapper> DeleteKeyAsync(string key);
}