using Common.Extensions.NpOn.CommonEnums;
using Common.Infrastructures.NpOn.CommonDb.DbResults;
using Common.Infrastructures.NpOn.DbFactory.Generics;
using Common.Infrastructures.NpOn.RedisExtCm.Commands;
using Common.Infrastructures.NpOn.RedisExtCm.Results;
using StackExchange.Redis;

namespace Common.Infrastructures.NpOn.DbFactory.Redis;

public class RedisFactoryWrapper : DbFactoryWrapper, IRedisFactoryWrapper
{
    public RedisFactoryWrapper(string connectString, EDb dbType, int connectionNumber = 1, bool isAutoOpen = false)
        : base(connectString, dbType, connectionNumber, isAutoOpen)
    {
    }

    #region Single Operations

    #region Generic Type

    public Task<INpOnWrapperResult?> GetAsync(string key)
    {
        var command = new RedisDbCommand(key, ERedisCommand.Get);
        return ExecuteAsync(command);
    }

    public Task<INpOnWrapperResult?> SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        var command = new RedisDbCommand(key, ERedisCommand.Set, value, expiry ?? TimeSpan.FromMinutes(5));
        return ExecuteAsync(command);
    }

    public Task<INpOnWrapperResult?> DeleteAsync(string key)
    {
        var command = new RedisDbCommand(key, ERedisCommand.Delete);
        return ExecuteAsync(command);
    }

    #endregion Generic Type

    #region Redis Wrapper Type

    public async Task<RedisValueWrapper?> GetStringAsync(string key)
    {
        var result = await GetAsync(key);
        return result as RedisValueWrapper;
    }

    public async Task<RedisValueWrapper?> SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        var result = await SetAsync(key, value, expiry ?? TimeSpan.FromMinutes(5));
        return result as RedisValueWrapper;
    }

    public async Task<RedisValueWrapper?> DeleteKeyAsync(string key)
    {
        var result = await DeleteAsync(key);
        return result as RedisValueWrapper;
    }

    #endregion Redis Wrapper Type

    #endregion Single Operations


    #region Bulk Operations

    #region Generic Type

    public async Task<INpOnWrapperResult?> GetManyAsync(params string[] keys)
    {
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        var command = new RedisDbCommand(ERedisCommand.GetMany, redisKeys);
        var result = await ExecuteAsync(command);
        return result;
    }

    public async Task<INpOnWrapperResult?> SetManyAsync(Dictionary<string, string> keyValues, TimeSpan? expiry = null)
    {
        var pairs = keyValues
            .Select(kvp => new KeyValuePair<RedisKey, RedisValue>(kvp.Key, kvp.Value))
            .ToArray();
        var command = new RedisDbCommand(pairs, expiry ?? TimeSpan.FromMinutes(5));
        var result = await ExecuteAsync(command);
        return result;
    }

    public async Task<INpOnWrapperResult?> DeleteManyAsync(params string[] keys)
    {
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        var command = new RedisDbCommand(ERedisCommand.DeleteMany, redisKeys);
        var result = await ExecuteAsync(command);
        return result;
    }

    #endregion Generic Type

    #region Redis Wrapper Type

    public async Task<RedisValueWrapper?> GetManyStringAsync(params string[] keys)
    {
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        var command = new RedisDbCommand(ERedisCommand.GetMany, redisKeys);
        var result = await ExecuteAsync(command);
        return result as RedisValueWrapper;
    }

    public async Task<RedisValueWrapper?> SetManyStringAsync(Dictionary<string, string> keyValues,
        TimeSpan? expiry = null)
    {
        var pairs = keyValues
            .Select(kvp => new KeyValuePair<RedisKey, RedisValue>(kvp.Key, kvp.Value))
            .ToArray();
        var command = new RedisDbCommand(pairs, expiry ?? TimeSpan.FromMinutes(5));
        var result = await ExecuteAsync(command);
        return result as RedisValueWrapper;
    }

    public async Task<RedisValueWrapper?> DeleteManyStringAsync(params string[] keys)
    {
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        var command = new RedisDbCommand(ERedisCommand.DeleteMany, redisKeys);
        var result = await ExecuteAsync(command);
        return result as RedisValueWrapper;
    }

    #endregion Redis Wrapper Type

    #endregion
}