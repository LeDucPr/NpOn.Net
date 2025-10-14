using CassandraExtCm.Connections;
using CommonDb.Connections;
using CommonDb.DbResults;
using DbFactory;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using PostgresExtCm.Connections;

namespace SystemController;

class Program
{
    static void Main(string[] args)
    {
        string initializationConnectString =
            "Host=localhost;Port=5432;Database=np_on_db;Username=postgres;Password=password";
        // kết nối đầu tiên được tạo bởi Pg (when deploy on server)
        IDbFactoryWrapper factoryWrapper = new DbFactoryWrapper(initializationConnectString, EDb.Postgres);
        ConnectionCtrl connectionCtrlDecoy = new ConnectionCtrl() // starter (chim mồi)
        {
            Id = 2,
            ConnectionInfoId = 0,
        };

        HashSet<string?> connectionStrings = new HashSet<string?>();
        connectionStrings.Add(initializationConnectString);

        InitialConnection initialConnection = new InitialConnection(factoryWrapper, connectionCtrlDecoy);
        BaseCtrl ctrl = initialConnection.InitializationObject;
        // var aaaaaaaaaa = initialConnection.FirstInitializationSessionId?.GetLookupData();

        if (ctrl is not ConnectionCtrl connectionCtrl)
            return;
        ConnectionInfoCtrl? connectionInfoCtrl = connectionCtrl.ConnectionInfo;
        int countConnection = connectionStrings.Count;
        if (string.IsNullOrWhiteSpace(connectionInfoCtrl?.ConnectString))
            return;
        connectionStrings.Add(connectionInfoCtrl.ConnectString);
        if (connectionStrings.Count == countConnection)
            return;

        INpOnConnectOptions postgresOptions = new PostgresConnectOptions()
            .SetConnectionString(connectionInfoCtrl.ConnectString);
        if (connectionInfoCtrl.DatabaseType == null)
            return;
        EDb dbType = (EDb)connectionInfoCtrl.DatabaseType!;

        INpOnConnectOptions? connectOption = null;
        if (dbType == EDb.Cassandra || dbType == EDb.ScyllaDb)
        {
            if (connectionInfoCtrl.Server != null && !string.IsNullOrWhiteSpace(connectionInfoCtrl.Server.Host)
                                                  && !string.IsNullOrWhiteSpace(connectionInfoCtrl.DatabaseName))
            {
                connectOption = new CassandraConnectOptions()
                    .SetContactAddresses<CassandraDriver>([connectionInfoCtrl.Server.Host])?
                    .SetConnectionString(connectionInfoCtrl.ConnectString)
                    .SetKeyspace<CassandraDriver>(connectionInfoCtrl.DatabaseName.ToLower());
            }
        }

        IDbFactoryWrapper? dbFactoryWrapper = null;
        if (connectOption != null)
        {
            dbFactoryWrapper = new DbFactoryWrapper(connectOption, dbType);
            INpOnWrapperResult? resultOfQuery =
                dbFactoryWrapper.QueryAsync("select * from SEMAST limit 10").GetAwaiter().GetResult();
            Console.WriteLine($"Thời gian thực thi: {resultOfQuery?.QueryTimeMilliseconds} ms");
            //
            INpOnWrapperResult? resultOfQuery3 =
                dbFactoryWrapper.QueryAsync("select * from SEMAST limit 100").GetAwaiter().GetResult();
            Console.WriteLine($"Thời gian thực thi: {resultOfQuery3?.QueryTimeMilliseconds} ms");
            // 
            INpOnWrapperResult? resultOfQuery2 =
                dbFactoryWrapper.QueryAsync("select * from SEMAST ").GetAwaiter().GetResult();
            Console.WriteLine($"Thời gian thực thi: {resultOfQuery2?.QueryTimeMilliseconds} ms");
        }
    }
}