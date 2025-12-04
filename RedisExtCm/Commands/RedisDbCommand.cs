using CommonDb.DbCommands;
using CommonObject;
using Enums;
using StackExchange.Redis;

namespace RedisExtCm.Commands;

public class RedisDbCommand : NpOnDbCommand
{
    private readonly EDb _dbType = EDb.Redis;
    public ERedisCommand CommandType { get; }
    public string Key { get; }
    public RedisValue Value { get; }
    public RedisKey[]? Keys { get; }
    public KeyValuePair<RedisKey, RedisValue>[]? KeyValues { get; }

    public RedisDbCommand(string key, ERedisCommand command, RedisValue value = default) : base(EDb.Redis,
        $"{command} {key}")
    {
        CommandType = command;
        Key = key;
        Value = value;
    }

    public RedisDbCommand(ERedisCommand command, RedisKey[] keys) : base(EDb.Redis,
        $"{command} {keys.Select(x => x.ToString()).AsArrayJoin()}")
    {
        CommandType = command;
        Keys = keys;
    }

    // Constructor for SetMany
    public RedisDbCommand(KeyValuePair<RedisKey, RedisValue>[] keyValues) : base(EDb.Redis,
        $"{ERedisCommand.SetMany} {keyValues.Select(x => x.Key.ToString()).AsArrayJoin()}")

    {
        CommandType = ERedisCommand.SetMany;
        KeyValues = keyValues;
    }
}