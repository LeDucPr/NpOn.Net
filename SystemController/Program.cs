using CassandraExtCm.Connections;
using CommonDb.Connections;
using CommonDb.DbResults;
using DbFactory;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using HandlerFlow.AlgObjs.CtrlObjs.Data;
using HandlerFlow.AlgObjs.RaisingRouters;
using Microsoft.Identity.Client;
using PostgresExtCm.Connections;
using SystemController.FactoryInitialzations;
using SystemController.ResultConverters;
using SystemController.ResultConverters.Domain;

namespace SystemController;

class Program
{
    private const int AmountConnections = 5;
    private const bool UseCachingSession = true;
    private const EDb DbTypeForFirstCreation = EDb.Postgres;

    [Obsolete("Obsolete")]
    static void Main(string[] args)
    {
        // kết nối đầu tiên được tạo bởi Pg (when deploy on server)
        IDbFactoryWrapper factoryWrapper = new DbFactoryWrapper(
            "Host=localhost;Port=5432;Database=np_on_db;Username=postgres;Password=password", DbTypeForFirstCreation,
            AmountConnections, UseCachingSession);
        ConnectionCtrl connectionCtrlDecoy = new ConnectionCtrl() // starter (chim mồi)
        {
            Id = 1, /*1: Postgres, 2: Scylla*/
            ConnectionInfoId = 0,
            QueryLanguageUse = (EDbLanguage)0,
        };

        (string? sessionId, BaseCtrl? ctrl) =
            factoryWrapper.GetDataWithConnection(connectionCtrlDecoy).GetAwaiter().GetResult();
        if (sessionId == null || ctrl == null)
            return;
        // var aaaaaaaaaa = sessionId.GetLookupData(); // List(output detail with foreach)

        if (ctrl is not ConnectionCtrl connectionCtrl)
            return;
        ConnectionInfoCtrl? connectionInfoCtrl = connectionCtrl.ConnectionInfo;
        if (connectionInfoCtrl == null)
            return;
        IDbFactoryWrapper? dbFactoryWrapper =
            InitializationCtrlSystem.CreateDbFactoryWrapper(connectionInfoCtrl).Result;

        
        
        // string pgQuery = "Select * from table_field_ctrl where table_id = 1";
        // INpOnWrapperResult? resultOfQuery = dbFactoryWrapper?.QueryAsync(pgQuery).GetAwaiter().GetResult();
        //
        // if (resultOfQuery is INpOnTableWrapper tableWrapper && tableWrapper.RowWrappers.Count > 0)
        //     for (int rowIndex = 0; rowIndex < tableWrapper.RowWrappers.Count; rowIndex++)
        //     {
        //         foreach (var rowCell in tableWrapper.RowWrappers[rowIndex]!.GetRowWrapper())
        //             Console.WriteLine(
        //                 $"RowsCell[index] = {rowIndex} --- {rowCell.Key}: Type- {rowCell.Value.DbType};  Value - {rowCell.Value.ValueAsObject}");
        //
        //         Console.WriteLine();
        //     }
        //
        // var tableFieldCtrls = resultOfQuery?.GenericConverter(typeof(TableFieldCtrl))?.ToList();
        // if (tableFieldCtrls is not { Count: > 0 })
        //     return;
        //
        //
        // (string? _, List<BaseCtrl>? tableFieldCtrlsOutput) =
        //     factoryWrapper.GetDataWithConnection(tableFieldCtrls).GetAwaiter().GetResult();
        //
        // if (tableFieldCtrlsOutput is not { Count: > 0 } || tableFieldCtrlsOutput.First() is not TableFieldCtrl)
        //     return;
        //
        // List<TableFieldCtrl> tableFieldCtrlList = tableFieldCtrlsOutput.Cast<TableFieldCtrl>().ToList();


        ////

        string pgQuery_unified_table_mapping_ctrl = "Select * from unified_table_mapping_ctrl";
        INpOnWrapperResult? resultOfQueryTableFieldMappingCtrls =
            dbFactoryWrapper?.QueryAsync(pgQuery_unified_table_mapping_ctrl).GetAwaiter().GetResult();
        List<BaseCtrl>? unifiedTableFieldMappingCtrls = resultOfQueryTableFieldMappingCtrls
            ?.GenericConverter(typeof(UnifiedTableMappingCtrl))?.ToList();
        if (unifiedTableFieldMappingCtrls is not { Count: > 0 })
            return;
        
        (string? _, unifiedTableFieldMappingCtrls) =
            factoryWrapper.GetDataWithConnection(unifiedTableFieldMappingCtrls).GetAwaiter().GetResult();

        var aaa = resultOfQueryTableFieldMappingCtrls?.JoiningTable().GetAwaiter().GetResult();
        
        
        
        
        int a = 1;

        // if (tableFieldCtrlList)


        ///// test ConnectionCtrl.Id = 2 // ScyllaDb 
        // INpOnWrapperResult? resultOfQuery =
        //     dbFactoryWrapper?.QueryAsync("select * from SEMAST limit 1000").GetAwaiter().GetResult();
        //
        // if (resultOfQuery is INpOnTableWrapper tableWrapper && tableWrapper.RowWrappers.Count > 0)
        // {
        //     foreach (var rowCell in tableWrapper.RowWrappers[0]!.GetRowWrapper())
        //     {
        //         Console.WriteLine(
        //             $"RowsCell --- {rowCell.Key}: Type- {rowCell.Value.DbType};  Value - {rowCell.Value.ValueAsObject}");
        //     }
        // }
        //
        // Console.WriteLine($"Thời gian thực thi: {resultOfQuery?.QueryTimeMilliseconds} ms");
    }
}