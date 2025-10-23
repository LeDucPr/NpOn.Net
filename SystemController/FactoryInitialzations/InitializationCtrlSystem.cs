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

namespace SystemController.FactoryInitialzations;

public static class InitializationCtrlSystem
{
    /// <summary>
    /// GetDataWithConnection() => SessionId.GetLookupData() when RaisingIndex.Joining(List)Data.isUseCachingForLookupData == true 
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="decoyObject"></param>
    /// <param name="isUseCache"></param>
    /// <returns></returns>
    public static async Task<(string? SessionId, BaseCtrl? Ctrl)> GetDataWithConnection
        (this IDbFactoryWrapper factory, BaseCtrl decoyObject, bool isUseCache = false)
    {
        (string? sessionId, BaseCtrl? ctrl) =
            (await decoyObject.JoiningData(
                ((ctrl) =>
                {
                    BaseQueryCreatorWithKey queryCreator = new BaseQueryCreatorWithKey(ctrl);
                    return Task.FromResult(queryCreator.CreateQueryWithId(factory.DbType));
                }),
                (async (query, type) =>
                {
                    INpOnWrapperResult? result = await factory.QueryAsync(query);
                    // var ctrl = result?.PostgresConverter(type);
                    var ctrl = result?.GenericConverter(type);
                    return ctrl?.FirstOrDefault();
                }),
                true, isUseCache, -1));
        if (sessionId != null && ctrl != null)
            return (sessionId, ctrl);
        return (null, null);
    }

    /// <summary>
    /// Retrieve multiple objects (inherit from BaseCtrl), including objects of different types
    /// GetDataWithConnection() => SessionId.GetLookupData() when RaisingIndex.Joining(List)Data.isUseCachingForLookupData == true 
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="decoyObjects"></param>
    /// <param name="isUseCache"></param>
    /// <returns></returns>
    public static async Task<(string? SessionId, List<BaseCtrl>? Ctrls)> GetDataWithConnection
        (this IDbFactoryWrapper factory, List<BaseCtrl> decoyObjects, bool isUseCache = false)
    {
        (string? sessionId, List<BaseCtrl>? ctrls) =
            (await decoyObjects.JoiningListData(
                ((ctrls) =>
                {
                    BaseQueryCreatorWithKey queryCreator = new BaseQueryCreatorWithKey(ctrls);
                    return Task.FromResult(queryCreator.CreateQueryWithIds(factory.DbType));
                }),
                (async (query, type) =>
                {
                    INpOnWrapperResult? result = await factory.QueryAsync(query);
                    // var ctrl = result?.PostgresConverter(type);
                    var ctrls = result?.GenericConverter(type);
                    return ctrls?.ToList();
                }),
                true, false, -1));
        if (sessionId != null && ctrls != null)
            return (sessionId, ctrls);
        return (null, null);
    }


    /// <summary>
    /// sử dụng tạo kết nối mới tới Database, đã tồn tại sử dụng kết nối cũ
    /// (available free time connection)
    /// </summary>
    /// <param name="connectionInfoCtrl">Inherits from BaseCtrl</param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public static Task<IDbFactoryWrapper?> CreateDbFactoryWrapper(ConnectionInfoCtrl connectionInfoCtrl)
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