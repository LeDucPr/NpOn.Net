using DbFactory;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using HandlerFlow.AlgObjs.RaisingRouters;

namespace SystemController;

class Program
{
    static void Main(string[] args)
    {
        IDbFactoryWrapper factoryWrapper =
            new DbFactoryWrapper(
                "Host=localhost;Port=5432;Database=np_on_db;Username=postgres;Password=password",
                EDb.Postgres
            );
        
        ConnectionCtrl connectionCtrlDecoy = new ConnectionCtrl() // starter (chim mồi)
        {
            Id = 1,
            ConnectionInfoId = 1,
        };

        InitialConnection initialConnection = new InitialConnection(factoryWrapper, connectionCtrlDecoy);
        BaseCtrl ctrl = initialConnection.InitializationObject;
        // var aaaaaaaaaa = initialConnection.FirstInitializationSessionId?.GetLookupData();
    }
}