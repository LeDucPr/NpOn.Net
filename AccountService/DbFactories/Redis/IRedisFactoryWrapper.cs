using DbFactory;
using RedisExtCm.Results;

namespace AccountService.DbFactories.Redis;

public interface IRedisFactoryWrapper : IDbFactoryWrapper
{
    Task<RedisValueWrapper> GetStringAsync(string key);
    Task<RedisValueWrapper> SetStringAsync(string key, string value);
    Task<RedisValueWrapper> DeleteKeyAsync(string key);
}