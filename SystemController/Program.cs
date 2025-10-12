using CommonDb.DbResults;
using DbFactory;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using SystemController.ResultConverters;

namespace SystemController;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        DbFactoryIntegrationTest().GetAwaiter();
    }

    private static async Task<INpOnWrapperResult?> DbFactoryIntegrationTest()
    {
        IDbFactoryWrapper wrapper =
            new DbFactoryWrapper(
                "Host=localhost;Port=5432;Database=np_on_db;Username=postgres;Password=password",
                EDb.Postgres
            );

        ConnectionCtrl connection = new ConnectionCtrl()
        {
            Id = 1,
            ConnectionInfoId = 2,
        };
        var resultTryGet = await wrapper.QueryAsync(StaticCommands.ConnectionCtrlGetAll);
        var ctrl = resultTryGet?.Converter(typeof(ConnectionCtrl));
        return resultTryGet;
    }
}