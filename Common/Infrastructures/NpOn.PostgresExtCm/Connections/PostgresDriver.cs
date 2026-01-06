using System.Data;
using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.CommonDb.Connections;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;
using Common.Infrastructures.NpOn.CommonDb.DbResults;
using Common.Infrastructures.NpOn.PostgresExtCm.Results;
using Npgsql;
using NpgsqlTypes;

namespace Common.Infrastructures.NpOn.PostgresExtCm.Connections;

public class PostgresDriver : NpOnDbDriver
{
    private NpgsqlConnection? _connection;
    public sealed override string Name { get; set; }
    public sealed override string Version { get; set; }

    public override bool IsValidSession => _connection is { State: ConnectionState.Open };

    public PostgresDriver(INpOnConnectOption option) : base(option)
    {
    }

    public override async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (IsValidSession)
        {
            return; // Đã kết nối rồi và option yêu cầu chờ.
        }

        await DisconnectAsync();
        _connection = new NpgsqlConnection(Option.ConnectionString);
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
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    // public   async Task<>
    
    public override async Task<INpOnWrapperResult> Execute(INpOnDbCommand? command)
    {
        // Kiểm tra trạng thái kết nối hợp lệ.
        if (!IsValidSession || _connection == null)
            return new PostgresResultSetWrapper().SetFail(EDbError.Connection);
        if (command == null || string.IsNullOrWhiteSpace(command.CommandText))
            return new PostgresResultSetWrapper().SetFail(EDbError.Command);
        try
        {
            if (command.Parameters is not { Count : > 0 })
            {
                await using var pgCommand = _connection.CreateCommand();
                pgCommand.CommandText = command.CommandText;
                await using var readerCm = await pgCommand.ExecuteReaderAsync();
                return new PostgresResultSetWrapper(readerCm);
            }

            using (var pgCommandParam = new NpgsqlCommand(command.CommandText, _connection))
            {
                foreach (var prm in command.Parameters)
                {
                    if (prm is not NpOnDbCommandParam<NpgsqlDbType> npgsqlParam)
                    {
                        return new PostgresResultSetWrapper().SetFail(EDbError.CommandParam);
                    }
                    
                    string newKey = npgsqlParam.ParamName.AsDefaultString();
                    object? paramValue = prm.ParamValue;

                    if (paramValue == null)
                    {
                        pgCommandParam.Parameters.AddWithValue(newKey, npgsqlParam.ParamType, DBNull.Value);
                        continue;
                    }

                    pgCommandParam.Parameters.Add(npgsqlParam.CreateNpgsqlParameter());
                }

                await using var readerCmPrm = await pgCommandParam.ExecuteReaderAsync();
                return new PostgresResultSetWrapper(readerCmPrm);
            }
        }
        catch (Exception ex)
        {
            return new PostgresResultSetWrapper().SetFail(ex);
        }
    }

    public override async Task<INpOnWrapperResult> ExecuteFunc(INpOnDbExecCommand? execCommand)
    {
        if (!IsValidSession || _connection == null) // Check enabled connection 
            return new PostgresResultSetWrapper().SetFail(EDbError.Connection);
        if (execCommand == null || string.IsNullOrWhiteSpace(execCommand.FuncName))
            return new PostgresResultSetWrapper().SetFail(EDbError.ExecFuncName);
        try
        {
            await using var pgCommand = _connection.CreateCommand();
            List<object?> paramPlaceholderComponents = new List<object?>();
            foreach (var param in execCommand.Params)
            {
                var value = param.Value;
                var npgsqlParam = new NpgsqlParameter
                {
                    ParameterName = param.Key,
                    Value = value
                };

                if (value != DBNull.Value)
                {
                    var valueType = value.GetType();
                    var npgsqlDbType = valueType.ToNpgsqlDbType(); // ?? ưu tiên 
                    if (npgsqlDbType.HasValue)
                        npgsqlParam.NpgsqlDbType = npgsqlDbType.Value;
                    paramPlaceholderComponents.Add(value);
                }
            }

            string paramPlaceholders = $"'{string.Join(",", paramPlaceholderComponents)}'";
            pgCommand.CommandText = $"SELECT * FROM {execCommand.FuncName}({paramPlaceholders})";
            pgCommand.CommandType = CommandType.Text;
            if (string.IsNullOrWhiteSpace(execCommand.AliasForSingleColumnOutput))
                pgCommand.CommandText += $" as {execCommand.AliasForSingleColumnOutput}";
            await using var reader = await pgCommand.ExecuteReaderAsync();
            return new PostgresResultSetWrapper(reader);
        }
        catch (Exception ex)
        {
            return new PostgresResultSetWrapper().SetFail(ex);
        }
    }

    public override async Task<INpOnWrapperResult> ExecuteFuncParams<TEnum>(INpOnDbExecCommand? execCommand,
        List<INpOnDbCommandParam<TEnum>> parameters)
    {
        if (typeof(TEnum) != typeof(NpgsqlDbType))
        {
            return new PostgresResultSetWrapper().SetFail(new Exception($"{typeof(TEnum).Name} is not NpgsqlDbType"));
        }

        if (!IsValidSession || _connection == null) // Check enabled connection 
            return new PostgresResultSetWrapper().SetFail(EDbError.Connection);
        if (execCommand == null || string.IsNullOrWhiteSpace(execCommand.FuncName))
            return new PostgresResultSetWrapper().SetFail(EDbError.ExecFuncName);
        List<string> paramList = [];
        try
        {
            await using var pgCommand = _connection.CreateCommand();
            foreach (var param in parameters)
            {
                var value = param.ParamValue ?? DBNull.Value;
                var npgsqlParam = new NpgsqlParameter
                {
                    ParameterName = param.ParamName,
                    NpgsqlDbType = (NpgsqlDbType)Enum.ToObject(typeof(NpgsqlDbType), param.ParamType),
                    Value = value
                };
                pgCommand.Parameters.Add(npgsqlParam);
                paramList.Add($"@{param.ParamName}");
            }

            string funcName = execCommand.FuncName.Trim().AsDefaultString();
            if (funcName == execCommand.FuncName)
                funcName = $"select * from {funcName}({paramList.AsArrayJoin()})";
            pgCommand.CommandText = funcName;
            await using var reader = await pgCommand.ExecuteReaderAsync();
            return new PostgresResultSetWrapper(reader);
        }
        catch (Exception ex)
        {
            return new PostgresResultSetWrapper().SetFail(ex);
        }
    }
}