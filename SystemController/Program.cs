using CassandraExtCm.Connections;
using CommonDb.Connections;
using CommonDb.DbResults;
using DbFactory;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using Microsoft.Identity.Client;
using PostgresExtCm.Connections;

namespace SystemController;

class Program
{
    private const int AmountConnections = 5;
    private const bool UseCachingSession = true;

    [Obsolete("Obsolete")]
    static void Main(string[] args)
    {
        // kết nối đầu tiên được tạo bởi Pg (when deploy on server)
        IDbFactoryWrapper factoryWrapper = new DbFactoryWrapper(
            "Host=localhost;Port=5432;Database=np_on_db;Username=postgres;Password=password", EDb.Postgres,
            AmountConnections, UseCachingSession);
        ConnectionCtrl connectionCtrlDecoy = new ConnectionCtrl() // starter (chim mồi)
        {
            Id = 2,
            ConnectionInfoId = 0,
            QueryLanguageUse = (EDbLanguage)0,
        };

        InitialConnection initialConnection = new InitialConnection(factoryWrapper, connectionCtrlDecoy);
        BaseCtrl ctrl = initialConnection.InitializationObject;
        // var aaaaaaaaaa = initialConnection.FirstInitializationSessionId?.GetLookupData(); // List(output detail with foreach)

        if (ctrl is not ConnectionCtrl connectionCtrl)
            return;
        ConnectionInfoCtrl? connectionInfoCtrl = connectionCtrl.ConnectionInfo;
        if (connectionInfoCtrl == null)
            return;
        IDbFactoryWrapper? dbFactoryWrapper = initialConnection.CreateDbFactoryWrapper(connectionInfoCtrl).Result;


        INpOnWrapperResult? resultOfQuery =
            dbFactoryWrapper?.QueryAsync("select * from SEMAST limit 1000").GetAwaiter().GetResult();
        if (resultOfQuery is INpOnTableWrapper tableWrapper && tableWrapper.RowWrappers.Count > 0)
        {
            foreach (var rowCell in tableWrapper.RowWrappers[0]!.GetRowWrapper())
            {
                Console.WriteLine(
                    $"RowsCell --- {rowCell.Key}: Type- {rowCell.Value.DbType};  Value - {rowCell.Value.ValueAsObject}");
            }
        }

        Console.WriteLine($"Thời gian thực thi: {resultOfQuery?.QueryTimeMilliseconds} ms");
    }
}