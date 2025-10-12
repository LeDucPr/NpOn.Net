using CommonDb.DbResults;
using DbFactory;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using HandlerFlow.AlgObjs.RaisingRouters;
using HandlerFlow.AlgObjs.SqlQueries;
using SystemController.ResultConverters;

namespace SystemController;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        DbFactoryIntegrationTest().GetAwaiter();
    }

    private static async Task<IEnumerable<DataLookup>?> DbFactoryIntegrationTest()
    {
        IDbFactoryWrapper factoryWrapper =
            new DbFactoryWrapper(
                "Host=localhost;Port=5432;Database=np_on_db;Username=postgres;Password=password",
                EDb.Postgres
            );


        ConnectionCtrl connectionCtrl = new ConnectionCtrl() // starter (chim mồi)
        {
            Id = 1,
            ConnectionInfoId = 1,
        };


        Func<Type, Task<string>> createStringQueryMethod = async (type) =>
        {
            BaseQueryCreatorWithKey queryCreator = new BaseQueryCreatorWithKey(connectionCtrl);
            return queryCreator.CreateQueryWithId();
        };

        Func<string, Type, Task<BaseCtrl?>> getDataMethod = async (query, type) =>
        {
            INpOnWrapperResult? result = await factoryWrapper.QueryAsync(query);
            var ctrl = result?.Converter(type);
            return ctrl?.FirstOrDefault();
        };
        // var resultTryGet = await wrapper.QueryAsync(StaticCommands.ConnectionCtrlGetAll);
        // var ctrl = resultTryGet?.Converter(typeof(ConnectionCtrl));
        // return resultTryGet;
        string? sessionId = await connectionCtrl.JoiningData(createStringQueryMethod, getDataMethod, true, -1);
        if (sessionId == null)
        {
            return null;
        }

        var lookupData = sessionId?.GetLookupData();
        return lookupData?.Data;
    }
}