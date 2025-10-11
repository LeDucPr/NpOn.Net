using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using HandlerFlow.AlgObjs.RaisingRouters;
using HandlerFlow.AlgObjs.SqlQueries;

namespace HandlerFlow;

class Program
{
    static void Main(string[] args)
    {
        Test().GetAwaiter();
    }

    private static async Task Test()
    {
        ServerCtrl server = new ServerCtrl() { Id = "1", };
        ConnectionInfoCtrl? ctrlInfo  = new ConnectionInfoCtrl()
        {
            Id = "2",
        };
        ConnectionCtrl? ctrl = new ConnectionCtrl()
        {
            Id = "1",
            ConnectionInfoId = "2"
        };
        var a = ctrlInfo.ForeignKeys()?.ToArray();
        var b = ctrlInfo.PrimaryKeys()?.ToArray();
        var c = ctrlInfo.ForeignKeyIds()?.ToArray();
        ctrlInfo = ctrlInfo.IsTableLoaderAttached() ? ctrlInfo : null;

        Func<Type, Task<string>> createStringQueryMethod = async (type) =>
        {
            BaseQueryCreatorWithKey queryCreator = new BaseQueryCreatorWithKey(ctrl);
            return queryCreator.CreateQueryWithId();
        };

        Func<string, Task<BaseCtrl>> getDataMethod = async (query) =>
        {
            return new ConnectionInfoCtrl { Id = "1", ServerId = "3"};
        };

        // Act
        await ctrl.JoiningData(createStringQueryMethod, getDataMethod, -1);
        
        

    }
}