using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;
using StackExchange.Redis;

namespace Common.Infrastructures.NpOn.RedisExtCm.Commands;

public class RedisDbCommand : NpOnDbCommand
{
    private readonly EDb _dbType = EDb.Redis;
    public ERedisCommand CommandType { get; }
    public string Key { get; }
    public RedisValue Value { get; }
    public RedisKey[]? Keys { get; }
    public KeyValuePair<RedisKey, RedisValue>[]? KeyValues { get; }
    public TimeSpan? Expiry { get; }

    public RedisDbCommand(string key, ERedisCommand command, RedisValue value = default, TimeSpan? expiry = null) :
        base(EDb.Redis, $"{command} {key}")
    {
        CommandType = command;
        Key = key;
        Value = value;
        Expiry = expiry;
    }
    
    // get/delete many
    public RedisDbCommand(ERedisCommand command, RedisKey[] keys) : base(EDb.Redis,
        $"{command} {keys.Select(x => x.ToString()).AsArrayJoin()}")
    {
        CommandType = command;
        Keys = keys;
    }

    // Constructor for SetMany
    public RedisDbCommand(KeyValuePair<RedisKey, RedisValue>[] keyValues, TimeSpan? expiry = null) : base(EDb.Redis,
        $"{ERedisCommand.SetMany} {keyValues.Select(x => x.Key.ToString()).AsArrayJoin()}")

    {
        CommandType = ERedisCommand.SetMany;
        Expiry = expiry;
        KeyValues = keyValues;
    }
}