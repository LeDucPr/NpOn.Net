using Common.Extensions.NpOn.CommonEnums;
using Common.Infrastructures.NpOn.CommonDb.Connections;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;
using Common.Infrastructures.NpOn.CommonDb.DbResults;
using Common.Infrastructures.NpOn.RedisExtCm.Commands;
using Common.Infrastructures.NpOn.RedisExtCm.Results;
using StackExchange.Redis;

namespace Common.Infrastructures.NpOn.RedisExtCm.Connections;

public class RedisDriver : NpOnDbDriver
{
    private ConnectionMultiplexer? _connection;
    public override string Name { get; set; } = "Redis";
    public override string Version { get; set; } = "Unknown";

    public override bool IsValidSession => _connection is { IsConnected: true };

    public RedisDriver(INpOnConnectOption option) : base(option)
    {
    }

    public override async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (IsValidSession)
        {
            return;
        }

        await DisconnectAsync();
        if (Option.ConnectionString != null)
            _connection = await ConnectionMultiplexer.ConnectAsync(Option.ConnectionString);

        if (_connection is { IsConnected: true })
        {
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            Version = server.Version.ToString();
            Name = $"Redis on {server.EndPoint}";
        }
    }

    public override async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public override async Task<INpOnWrapperResult> Execute(INpOnDbCommand? command)
    {
        if (!IsValidSession || _connection == null)
        {
            return new RedisValueWrapper(new RedisValueContainer(RedisValue.Null)).SetFail(EDbError.Connection);
        }

        if (command is not RedisDbCommand redisCommand)
        {
            return new RedisValueWrapper(new RedisValueContainer(RedisValue.Null))
                .SetFail(EDbError.CommandNotSupported);
        }

        try
        {
            IDatabase db = _connection.GetDatabase();
            return redisCommand.CommandType switch
            {
                ERedisCommand.Set => new RedisValueWrapper(new RedisValueContainer(
                    await db.StringSetAsync(redisCommand.Key, redisCommand.Value, redisCommand.Expiry))),
                ERedisCommand.Get => new RedisValueWrapper(new RedisValueContainer(
                    await db.StringGetAsync(redisCommand.Key))),
                ERedisCommand.Delete => new RedisValueWrapper(new RedisValueContainer(
                    await db.KeyDeleteAsync(redisCommand.Key))),
                ERedisCommand.GetMany when redisCommand.Keys != null => new RedisValueWrapper(new RedisValueContainer(
                    await db.StringGetAsync(redisCommand.Keys))),
                ERedisCommand.SetMany when redisCommand.KeyValues != null => await HandleSetMany(db, redisCommand),
                ERedisCommand.DeleteMany when redisCommand.Keys != null => new RedisValueWrapper(
                    new RedisValueContainer(await db.KeyDeleteAsync(redisCommand.Keys))),
                _ => new RedisValueWrapper(new RedisValueContainer(RedisValue.Null))
                    .SetFail(EDbError.CommandNotSupported)
            };
        }
        catch (Exception ex)
        {
            return new RedisValueWrapper(new RedisValueContainer(RedisValue.Null)).SetFail(EDbError.RedisExecute);
        }
    }

    private static async Task<RedisValueWrapper> HandleSetMany(IDatabase db, RedisDbCommand redisCommand)
    {
        // ManySET command itself doesn't support expiry. We use a transaction (batch) to achieve this.
        var batch = db.CreateBatch();
        foreach (var pair in redisCommand.KeyValues!)
        {
            _ = batch.StringSetAsync(pair.Key, pair.Value, redisCommand.Expiry);
        }

        batch.Execute();
        // Since we're using a batch, the result isn't directly available. We'll return success.
        // The actual set operations are "fire and forget" within the batch.
        return new RedisValueWrapper(new RedisValueContainer(true));
    }
}