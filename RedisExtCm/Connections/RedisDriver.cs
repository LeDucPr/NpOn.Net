using CommonDb.Connections;
using CommonDb.DbCommands;
using CommonDb.DbResults;
using Enums;
using RedisExtCm.Results;
using StackExchange.Redis;

namespace RedisExtCm.Connections;

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

    public override Task<INpOnWrapperResult> Query(INpOnDbCommand? command)
    {
        return Task.FromResult<INpOnWrapperResult>(
            new RedisValueWrapper(new RedisValueContainer(RedisValue.Null)).SetFail(EDbError.CommandNotSupported));
    }

    // public override Task<INpOnWrapperResult> ExecuteFunc(INpOnDbExecCommand? execCommand)
    // {
    //     // Có thể triển khai trong tương lai để gọi các lệnh Redis cụ thể
    //     return Task.FromResult<INpOnWrapperResult>(new RedisValueWrapper(RedisValue.Null).SetFail(EDbError.CommandNotSupported));
    // }
}