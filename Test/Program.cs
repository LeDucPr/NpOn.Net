using CommonDb.DbResults;
using DbFactory;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using HandlerFlow.AlgObjs.RaisingRouters;
using HandlerFlow.AlgObjs.SqlQueries;
using SystemController.ResultConverters;

namespace Test;

class Program
{
    
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        // DbFactoryIntegrationTest().GetAwaiter();
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
            QueryLanguageUse = (EDbLanguage)0,
        };


        Func<BaseCtrl, Task<string>> createStringQueryMethod = async (ctrl) =>
        {
            BaseQueryCreatorWithKey queryCreator = new BaseQueryCreatorWithKey(ctrl);
            return queryCreator.CreateQueryWithId();
        };

        Func<string, Type, Task<BaseCtrl?>> getDataMethod = async (query, type) =>
        {
            INpOnWrapperResult? result = await factoryWrapper.QueryAsync(query);
            var ctrl = result?.PostgresConverter(type);
            return ctrl?.FirstOrDefault();
        };
        (string? sessionId, BaseCtrl? connCtrl) = await connectionCtrl.JoiningData(createStringQueryMethod, getDataMethod, true, -1);
        if (sessionId == null)
        {
            return null;
        }

        JoinListLookup? lookupData = sessionId?.GetLookupData();
        return lookupData?.Data;
    }
}