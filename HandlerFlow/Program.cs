using Enums;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using HandlerFlow.AlgObjs.RaisingRouters;

namespace HandlerFlow;

class Program
{
    static void Main(string[] args)
    {
        ServerCtrl server = new ServerCtrl() { Id = "1", };
        ConnectionInfoCtrl ctrl = new ConnectionInfoCtrl()
        {
            Id = "1",
        };
        var a = ctrl.ForeignKeys()?.ToArray();
        var b = ctrl.PrimaryKeys()?.ToArray();
        var s = 1;
    }
}