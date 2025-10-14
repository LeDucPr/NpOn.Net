using CommonDb.Connections;
using DbFactory;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using HandlerFlow.AlgObjs.RaisingRouters;
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
            Id = 1,
            ConnectionInfoId = 1,
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
        IDbDriverFactory dbFactory = new DbDriverFactory(EDb.Postgres, postgresOptions);
        
    }
}