using CommonDb.DbCommands;
using Enums;
using StackExchange.Redis;

namespace RedisExtCm.Commands;

public class RedisDbCommand : NpOnDbCommand
{
    private readonly EDb _dbType = EDb.Redis;
    public ERedisCommand CommandType { get; }
    public string Key { get; }
    public RedisValue Value { get; }

    public RedisDbCommand(string key, ERedisCommand command, RedisValue value = default) : base(EDb.Redis,
        $"{command} {key}")
    {
        CommandType = command;
        Key = key;
        Value = value;
    }
}