using Algorithm.AlgObjs.CtrlObjs;
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
        DbDriverFactoryCreator factoryCreator = new DbDriverFactoryCreator(_dbType, openConnectString);
        _pgFactory = factoryCreator.GetDbDriverFactory;
    }

    public INpOnWrapperResult? GetAllCtrls()
    {
        if (_pgFactory == null) return null;
        if (_pgFactory.FirstValidConnection == null)
            _pgFactory.OpenConnections();
        if (_pgFactory.FirstValidConnection == null)
            return null;
        INpOnDbCommand command = new NpOnDbCommand(_dbType, StaticCommands.ConnectionCtrlGetAll);
        var result = _pgFactory.FirstValidConnection.Driver.Query(command).GetAwaiter().GetResult();
        return result;
    }
}