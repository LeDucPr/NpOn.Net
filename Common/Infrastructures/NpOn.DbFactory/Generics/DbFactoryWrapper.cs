using Common.Extensions.NpOn.CommonEnums;
using Common.Infrastructures.NpOn.CommonDb.Connections;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;
using Common.Infrastructures.NpOn.CommonDb.DbCommands.Extensions;
using Common.Infrastructures.NpOn.CommonDb.DbResults;
using Common.Infrastructures.NpOn.DbFactory.FactoryResults;

namespace Common.Infrastructures.NpOn.DbFactory.Generics;

public class DbFactoryWrapper : IDbFactoryWrapper
{
    private readonly IDbDriverFactory? _factory;
    private readonly EDb _dbType;

    public EDb DbType => _dbType;

    /// <summary>
    /// Tạo ra cho kết nối chỉ dùng ConnectionString hoặc lấy tham số khi khởi động
    /// </summary>
    /// <param name="openConnectString">Tham sô kết nối được mặc định cho khởi động là 1</param>
    /// <param name="dbType"></param>
    /// <param name="connectionNumber"></param>
    /// <param name="isUseCaching"></param>
    public DbFactoryWrapper(string openConnectString, EDb dbType, int connectionNumber = 1, bool isUseCaching = true)
    {
        _dbType = dbType;
        var factoryCreator =
            new DbDriverFactoryCreator(_dbType, openConnectString, connectionNumber);
        _factory = factoryCreator.GetDbDriverFactory;
        if (isUseCaching)
            this.AddToDbFactoryWrapperCache();
    }

    /// <summary>
    /// Generic initial
    /// </summary>
    /// <param name="connectOption"></param>
    /// <param name="dbType"></param>
    /// <param name="connectionNumber"></param>
    /// <param name="isUseCaching"></param>
    public DbFactoryWrapper(INpOnConnectOption connectOption, EDb dbType, int connectionNumber = 1,
        bool isUseCaching = true)
    {
        _dbType = dbType;
        _factory = new DbDriverFactory(dbType, connectOption, connectionNumber);
        if (isUseCaching)
            this.AddToDbFactoryWrapperCache();
    }

    public string? FactoryOptionCode => _factory?.DriverOptionKey;

    public async Task<INpOnWrapperResult?> ExecuteAsync(INpOnDbCommand dbCommand)
    {
        if (_factory == null) return null;
        NpOnDbConnection? connection = null;
        try
        {
            connection = await _factory.GetConnectionAsync();
            if (connection == null) return null; // Không có kết nối khả dụng
            return await connection.Driver.Execute(dbCommand);
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            if (connection != null) _factory.ReleaseConnection(connection);
        }
    }

    public async Task<INpOnWrapperResult?> ExecuteAsync(string queryString)
    {
        if (_factory == null) return null;
        NpOnDbConnection? connection = null;
        try
        {
            connection = await _factory.GetConnectionAsync();
            if (connection == null) return null;
            INpOnDbCommand command = new NpOnDbCommand(_dbType, queryString);
            return await connection.Driver.Execute(command);
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            if (connection != null) _factory.ReleaseConnection(connection);
        }
    }

    public async Task<INpOnWrapperResult?> ExecuteAsync(string queryString, List<NpOnDbCommandParam> parameters)
    {
        if (_factory == null) return null;
        NpOnDbConnection? connection = null;
        try
        {
            connection = await _factory.GetConnectionAsync();
            if (connection == null) return null;
            INpOnDbCommand command = new NpOnDbCommand(_dbType, queryString, parameters);
            return await connection.Driver.Execute(command);
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            if (connection != null) _factory.ReleaseConnection(connection);
        }
    }

    public async Task<INpOnWrapperResult?> ExecuteFunc(string funcName, Dictionary<string, object> parameters,
        bool isUseInputJson = false, string? isUseOutputJsonAsName = null)
    {
        if (_factory == null) return null;
        NpOnDbConnection? connection = null;
        try
        {
            connection = await _factory.GetConnectionAsync();
            if (connection == null) return null;
            INpOnDbExecCommand execCommand =
                new NpOnDbExecCommand(_dbType, funcName, parameters, isUseOutputJsonAsName);
            if (isUseInputJson)
                execCommand = execCommand.AsFullJsonBlock();
            return await connection.Driver.ExecuteFunc(execCommand);
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            if (connection != null) _factory.ReleaseConnection(connection);
        }
    }

    public async Task<INpOnWrapperResult?> ExecuteFuncParams<TEnumDbType>(string funcName,
        List<INpOnDbCommandParam<TEnumDbType>>? parameters) where TEnumDbType : Enum
    {
        if (_factory == null) return null;
        NpOnDbConnection? connection = null;
        try
        {
            connection = await _factory.GetConnectionAsync();
            if (connection == null) return null;
            INpOnDbExecCommand execCommand =
                new NpOnDbExecCommand(_dbType, funcName, parameters ?? []);
            return await connection.Driver.ExecuteFuncParams(execCommand, parameters ?? []);
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            if (connection != null) _factory.ReleaseConnection(connection);
        }
    }
}