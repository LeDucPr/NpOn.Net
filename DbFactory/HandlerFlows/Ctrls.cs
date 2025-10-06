using CommonDb.Connections;
using CommonDb.DbCommands;
using CommonDb.DbResults;
using DbFactory.FactoryResults;
using Enums;
using PostgresExtCm.Connections;

namespace DbFactory.HandlerFlows;

public class Ctrls
{
    private IDbDriverFactory? _pgFactory;
    private readonly EDb _dbType = EDb.Postgres;

    public Ctrls(string openConnectString)
    {
        DbDriverFactoryCreator factoryCreator = new DbDriverFactoryCreator(_dbType, openConnectString, 1);
        _pgFactory = factoryCreator.GetDbDriverFactory;
    }

#if DEBUG
    public async Task<INpOnWrapperResult?> GetAllCtrls()
    {
        if (_pgFactory == null) return null;
        if (_pgFactory.FirstValidConnection == null)
            await _pgFactory.OpenConnections();
        if (_pgFactory.FirstValidConnection == null)
            return null;
        INpOnDbCommand command = new NpOnDbCommand(_dbType, StaticCommands.ConnectionCtrlGetAll);
        INpOnWrapperResult result = _pgFactory.FirstValidConnection.Driver.Query(command).GetAwaiter().GetResult();
        return result;
    }
#endif
}