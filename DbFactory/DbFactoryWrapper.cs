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

    public DbFactoryWrapper(string openConnectString, EDb dbType)
    {
        _dbType = dbType;
        DbDriverFactoryCreator factoryCreator = new DbDriverFactoryCreator(_dbType, openConnectString, 1);
        _factory = factoryCreator.GetDbDriverFactory;
    }

    public async Task<INpOnWrapperResult?> QueryAsync()
    {
        if (_factory == null) return null;
        if (_factory.FirstValidConnection == null)
            await _factory.OpenConnections();
        if (_factory.FirstValidConnection == null)
            return null;
        try
        {
            INpOnDbCommand command = new NpOnDbCommand(_dbType, StaticCommands.ConnectionCtrlGetAll);
            INpOnWrapperResult result = _factory.FirstValidConnection.Driver.Query(command).GetAwaiter().GetResult();
            return result;
        }
        catch (Exception)
        {
            return null;
        }
    }
}