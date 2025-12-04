using DbFactory.Generics;
using Enums;
using RedisExtCm.Commands;
using RedisExtCm.Results;

namespace DbFactory.Redis;

public class RedisFactoryWrapper : DbFactoryWrapper, IRedisFactoryWrapper
{
    public RedisFactoryWrapper(string connectString, EDb dbType, int connectionNumber = 1, bool isAutoOpen = false)
        : base(connectString, dbType, connectionNumber, isAutoOpen)
    {
    }

    public async Task<RedisValueWrapper> GetStringAsync(string key)
    {
        var command = new RedisDbCommand(key, ERedisCommand.Get);
        var ccc = await ExecuteAsync(command);
        return (RedisValueWrapper) await ExecuteAsync(command);
    }

    public async Task<RedisValueWrapper> SetStringAsync(string key, string value)
    {
        var command = new RedisDbCommand(key, ERedisCommand.Set,  value);
        return (RedisValueWrapper)await ExecuteAsync(command);
    }

    public async Task<RedisValueWrapper> DeleteKeyAsync(string key)
    {
        var command = new RedisDbCommand(key, ERedisCommand.Delete);
        return (RedisValueWrapper)await ExecuteAsync(command);
    }
}