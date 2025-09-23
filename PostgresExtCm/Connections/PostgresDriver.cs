// File: D:/@C#/NpOn/PostgresExtCm/Connections/PostgresDriver.cs

using CommonDb.Connections;
using CommonDb.DbCommands;
using Npgsql; // Quan trọng: Sử dụng thư viện Npgsql
using System.Data;
using PostgresExtCm.Sql;

namespace PostgresExtCm.Connections;

public class PostgresDriver : NpOnDbDriver
{
    private NpgsqlConnection? _connection;
    public sealed override string Name { get; set; }
    public sealed override string Version { get; set; }

    public override bool IsValidSession => _connection is { State: ConnectionState.Open };

    public PostgresDriver(INpOnConnectOptions options) : base(options)
    {
    }

    public override async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (IsValidSession)
        {
            return; // Đã kết nối rồi và option yêu cầu chờ.
        }

        await DisconnectAsync();
        _connection = new NpgsqlConnection(Options.ConnectionString);
        await _connection.OpenAsync(cancellationToken);
        Version = _connection.PostgreSqlVersion.ToString();
        if (_connection.Host != null)
            Name = _connection.Host;
        else
            Name = $"PostgresSql {_connection.PostgreSqlVersion.Major}"; // ?????????????
    }

    public override async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            // Npgsql khuyến nghị gọi CloseAsync() trước khi DisposeAsync()
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public override async Task<INpOnDbResult> Query(INpOnDbCommand? command)
    {
        // Kiểm tra trạng thái kết nối hợp lệ.
        if (!IsValidSession || _connection == null)
            return new PostgresResult().SetFail("Connection is not yet open. Call ConnectAsync first");
        if (command == null || string.IsNullOrWhiteSpace(command.CommandText))
            return new PostgresResult().SetFail("Command cannot be empty or have a missing CommandText");
        try
        {
            await using var pgCommand = _connection.CreateCommand();
            pgCommand.CommandText = command.CommandText;
            await using var reader = await pgCommand.ExecuteReaderAsync();
            return await PostgresResult.CreateAsync(reader);
        }
        catch (Exception ex)
        {
            return new PostgresResult().SetFail(ex);
        }
    }
}