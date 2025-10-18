using CassandraExtCm.Connections;
using CommonDb.Connections;
using CommonDb.DbResults;
using DbFactory;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using HandlerFlow.AlgObjs.RaisingRouters;
using HandlerFlow.AlgObjs.SqlQueries;
using MongoDbExtCm.Connections;
using MssqlExtCm.Connections;
using PostgresExtCm.Connections;
using SystemController.ResultConverters;


namespace SystemController;

public class InitialConnection
{
    private readonly IDbFactoryWrapper _factory;
    public BaseCtrl InitializationObject { get; private set; }
    public string? FirstInitializationSessionId { get; private set; }

    public InitialConnection(IDbFactoryWrapper factory, BaseCtrl decoyObject, EDb dbType)
    {
        _factory = factory;
        InitializationObject = decoyObject;
        GetDataOfFirstConnection(dbType).GetAwaiter().GetResult();
    }

    /// <summary>
    /// FirstConnect When Start Program
    /// </summary>
    private async Task GetDataOfFirstConnection(EDb dbType = EDb.Postgres) // ConnectionCtrl : BasCtrl
    {
        (string? sessionId, BaseCtrl? ctrl) =
            (await InitializationObject.JoiningData(
                ((ctrl) =>
                {
                    BaseQueryCreatorWithKey queryCreator = new BaseQueryCreatorWithKey(ctrl);
                    return Task.FromResult(queryCreator.CreateQueryWithId(dbType));
                }),
                (async (query, type) =>
                {
                    INpOnWrapperResult? result = await _factory.QueryAsync(query);
                    // var ctrl = result?.PostgresConverter(type);
                    var ctrl = result?.GenericConverter(type);
                    return ctrl?.FirstOrDefault();
                }),
                true, -1));
        if (sessionId != null)
            FirstInitializationSessionId = sessionId;
        if (ctrl != null)
            InitializationObject = ctrl;
    }

    /// <summary>
    /// sử dụng tạo kết nối mới tới Database, đã tồn tại sử dụng kết nối cũ
    /// (available free time connection)
    /// </summary>
    /// <param name="connectionInfoCtrl">Inherits from BaseCtrl</param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public Task<IDbFactoryWrapper?> CreateDbFactoryWrapper(ConnectionInfoCtrl connectionInfoCtrl)
    {
        if (string.IsNullOrWhiteSpace(connectionInfoCtrl.ConnectString))
            return Task.FromResult<IDbFactoryWrapper?>(null);

        EDb dbType = connectionInfoCtrl.DatabaseType;
        INpOnConnectOption? connectOption = null;

        switch (dbType)
        {
            case EDb.Postgres:
                connectOption = new PostgresConnectOption()
                    .SetConnectionString(connectionInfoCtrl.ConnectString);
                break;
            case EDb.Mssql:
                connectOption = new MssqlConnectOption()
                    .SetConnectionString(connectionInfoCtrl.ConnectString);
                break;
            case EDb.MongoDb:
                if (!string.IsNullOrWhiteSpace(connectionInfoCtrl.DatabaseName))
                    connectOption = new MongoDbConnectOption()
                        .SetConnectionString(connectionInfoCtrl.ConnectString)
                        .SetDatabaseName(connectionInfoCtrl.DatabaseName);
                break;
            case EDb.Cassandra:
            case EDb.ScyllaDb:
                if (connectionInfoCtrl.Server != null && !string.IsNullOrWhiteSpace(connectionInfoCtrl.Server.Host)
                                                      && !string.IsNullOrWhiteSpace(connectionInfoCtrl.DatabaseName))
                    connectOption = new CassandraConnectOption()
                        .SetContactAddresses<CassandraDriver>([connectionInfoCtrl.Server.Host])?
                        .SetConnectionString(connectionInfoCtrl.ConnectString)
                        .SetKeyspace<CassandraDriver>(connectionInfoCtrl.DatabaseName.ToLower());
                break;
        }

        IDbFactoryWrapper? dbFactoryWrapper = null;
        if (connectOption != null)
        {
            try
            {
                string key = connectOption.Code;
                dbFactoryWrapper = DbFactorySessions.GetFactoryWrapperFromCache(key); // except when not found
            }
            catch (KeyNotFoundException)
            {
                dbFactoryWrapper = new DbFactoryWrapper(connectOption, dbType);
            }
        }

        return Task.FromResult(dbFactoryWrapper);
    }
}