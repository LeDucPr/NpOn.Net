using CommonDb.Connections;
using CommonDb.DbCommands;
using CommonDb.DbResults;
using DbFactory.FactoryResults;
using DbFactory.HandlerFlows;
using Enums;

namespace DbFactory;

public class DbFactoryWrapper : IDbFactoryWrapper
{
    private readonly IDbDriverFactory? _factory;
    private readonly EDb _dbType;

    /// <summary>
    /// Tạo ra cho kết nối chỉ dùng ConnectionString hoặc lấy tham số khi khởi động
    /// </summary>
    /// <param name="openConnectString">Tham sô kết nối được mặc định cho khởi động là 1</param>
    /// <param name="dbType"></param>
    /// <param name="connectionNumber"></param>
    public DbFactoryWrapper(string openConnectString, EDb dbType, int connectionNumber = 1) 
    {
        _dbType = dbType;
        DbDriverFactoryCreator factoryCreator = new DbDriverFactoryCreator(_dbType, openConnectString, connectionNumber);
        _factory = factoryCreator.GetDbDriverFactory;
    }

    /// <summary>
    /// Generic initial
    /// </summary>
    /// <param name="connectOptions"></param>
    /// <param name="dbType"></param>
    /// <param name="connectionNumber"></param>
    public DbFactoryWrapper(INpOnConnectOptions connectOptions, EDb dbType, int connectionNumber = 1)
    {
        _dbType = dbType;
        _factory = new DbDriverFactory(dbType, connectOptions, connectionNumber);
    }
    
    public async Task<INpOnWrapperResult?> QueryAsync(string queryString)
    {
        if (_factory == null) return null;
        if (_factory.FirstValidConnection == null)
            await _factory.OpenConnections();
        if (_factory.FirstValidConnection == null)
            return null;
        try
        {
            INpOnDbCommand command = new NpOnDbCommand(_dbType, queryString);
            INpOnWrapperResult result = _factory.FirstValidConnection.Driver.Query(command).GetAwaiter().GetResult();
            return result;
        }
        catch (Exception)
        {
            return null;
        }
    }
}