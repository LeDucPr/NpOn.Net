using CommonDb.DbResults;
using DbFactory;
using Enums;
using HandleFlow.ResultConverters;
using ObjectHandlerFlow.AlgObjs.CtrlObjs;
using ObjectHandlerFlow.AlgObjs.CtrlObjs.Connections;
using ObjectHandlerFlow.AlgObjs.RaisingRouters;
using ObjectHandlerFlow.AlgObjs.SqlQueries;

namespace Test.TestZones;

public static class DbFactoryIntegrationTest
{
    
    public static async Task<IEnumerable<DataLookup>?> DbFactoryIntegration()
    {
        EDb dbTypeForFirstCreation = EDb.Postgres;

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


        Func<SysBaseCtrl, Task<string>> createStringQueryMethod = async (ctrl) =>
        {
            BaseQueryCreatorWithKey queryCreator = new BaseQueryCreatorWithKey(ctrl);
            return queryCreator.CreateQueryWithId(dbTypeForFirstCreation);
        };

        Func<string, Type, Task<SysBaseCtrl?>> getDataMethod = async (query, type) =>
        {
            INpOnWrapperResult? result = await factoryWrapper.QueryAsync(query);
            var ctrl = result?.PostgresConverter(type);
            return ctrl?.FirstOrDefault();
        };
        (string? sessionId, SysBaseCtrl? connCtrl) =
            await connectionCtrl.JoiningData(createStringQueryMethod, getDataMethod, true, true, -1);
        if (sessionId == null)
        {
            return null;
        }

        JoinListLookup? lookupData = sessionId?.GetLookupData();
        return lookupData?.Data;
    }
}