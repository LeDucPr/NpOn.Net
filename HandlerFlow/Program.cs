using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using HandlerFlow.AlgObjs.RaisingRouters;

namespace HandlerFlow;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        ConnectionInfoCtrl ctrl = new ConnectionInfoCtrl()
        {
            Id = "1", 
        };
        RaisingIndexer.V1(ctrl);
    }
}