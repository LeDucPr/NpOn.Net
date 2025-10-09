using Enums;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using HandlerFlow.AlgObjs.RaisingRouters;

namespace HandlerFlow;

class Program
{
    static void Main(string[] args)
    {
        ServerCtrl server = new ServerCtrl() { Id = "1", };
        ConnectionInfoCtrl? ctrlInfo  = new ConnectionInfoCtrl()
        {
            Id = "1",
        };
        ConnectionCtrl? ctrl = new ConnectionCtrl()
        {
            Id = "1",
        };
        var a = ctrlInfo.ForeignKeys()?.ToArray();
        var b = ctrlInfo.PrimaryKeys()?.ToArray();
        var c = ctrlInfo.ForeignKeyIds()?.ToArray();
        ctrlInfo = ctrlInfo.IsTableLoaderAttached() ? ctrlInfo : null;

        ctrl.JoinList(null, null);
        
        var s = 1;
    }
}