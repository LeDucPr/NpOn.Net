// File: D:/@C#/NpOn/PostgresExtCm/Connections/PostgresDriver.cs

using CommonDb.Connections;
using CommonDb.DbCommands;
using Npgsql; // Quan trọng: Sử dụng thư viện Npgsql
using System.Data;

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
        if (!IsValidSession || _connection == null)
        {
            throw new InvalidOperationException("Kết nối chưa được mở. Hãy gọi ConnectAsync trước.");
        }

        if (command == null || string.IsNullOrWhiteSpace(command.CommandText))
        {
            throw new ArgumentNullException(nameof(command), "Command không được rỗng.");
        }

        //
        // await using var pgCommand = _connection.CreateCommand();
        // pgCommand.CommandText = command.CommandText;
        //
        // // Xử lý tham số cơ bản
        // if (command.Parameters != null)
        // {
        //     foreach (var param in command.Parameters)
        //     {
        //         pgCommand.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
        //     }
        // }
        //
        //
        //
        // await using var reader = await pgCommand.ExecuteReaderAsync();
        //
        // var results = new List<Dictionary<string, object?>>();
        // while (await reader.ReadAsync())
        // {
        //     var row = new Dictionary<string, object?>();
        //     for (int i = 0; i < reader.FieldCount; i++)
        //     {
        //         var columnName = reader.GetName(i);
        //         var value = reader.GetValue(i);
        //         row[columnName] = value == DBNull.Value ? null : value;
        //     }
        //
        //     results.Add(row);
        // }
        //
        // // Bạn sẽ cần một lớp triển khai INpOnDbResult cụ thể
        // return new NpOnDbResult(results, reader.RecordsAffected);

        return null;

    }
}