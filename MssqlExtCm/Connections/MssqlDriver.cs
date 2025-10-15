// File: C:/@LeDucGitOthers/Mine/NpOn.Net/MssqlExtCm/Connections/MssqlDriver.cs

using CommonDb.Connections;
using CommonDb.DbCommands;
using Microsoft.Data.SqlClient; // Important: Use Microsoft.Data.SqlClient library
using System.Data;
using CommonDb.DbResults;
using Enums;
using MssqlExtCm.Results;

namespace MssqlExtCm.Connections;

public class MssqlDriver : NpOnDbDriver
{
    private SqlConnection? _connection;
    public sealed override string Name { get; set; }
    public sealed override string Version { get; set; }

    public override bool IsValidSession => _connection is { State: ConnectionState.Open };

    public MssqlDriver(INpOnConnectOptions options) : base(options)
    {
        Name = "Mssql";
        Version = "0.0";
    }

    public override async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (IsValidSession)
        {
            return; // Already connected.
        }

        await DisconnectAsync();
        _connection = new SqlConnection(Options.ConnectionString);
        await _connection.OpenAsync(cancellationToken);
        Version = _connection.ServerVersion;
        if (!string.IsNullOrEmpty(_connection.DataSource))
            Name = _connection.DataSource;
        else
            Name = $"MSSQL Server {_connection.ServerVersion.Split('.')[0]}";
    }

    public override async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public override async Task<INpOnWrapperResult> Query(INpOnDbCommand? command)
    {
        // Check for a valid connection state.
        if (!IsValidSession || _connection == null)
            return new MssqlResultSetWrapper().SetFail(EDbError.Connection);
        if (command == null || string.IsNullOrWhiteSpace(command.CommandText))
            return new MssqlResultSetWrapper().SetFail(EDbError.Command);
        try
        {
            await using var sqlCommand = _connection.CreateCommand();
            sqlCommand.CommandText = command.CommandText;
            await using var reader = await sqlCommand.ExecuteReaderAsync();
            return new MssqlResultSetWrapper(reader);
        }
        catch (Exception ex)
        {
            return new MssqlResultSetWrapper().SetFail(ex);
        }
    }
}
