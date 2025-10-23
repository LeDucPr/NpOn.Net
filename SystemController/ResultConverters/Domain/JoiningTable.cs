using CommonDb;
using CommonDb.DbResults;
using CommonObject;
using DbFactory;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.CtrlObjs.Data;
using HandlerFlow.AlgObjs.SqlQueries;
using Npgsql;
using SystemController.FactoryInitialzations;

namespace SystemController.ResultConverters.Domain;

public static class JoiningTableExtensions
{
    [Obsolete("Obsolete")]
    public static async Task<INpOnWrapperResult?> JoiningTable(
        this INpOnWrapperResult result)
    {
        if (result is not INpOnTableWrapper tableWrapper)
            return null;

        List<BaseCtrl>? ctrls = result.GenericConverter(typeof(UnifiedTableMappingCtrl))?.ToList();
        if (ctrls is not { Count: > 0 })
            return null;

        List<UnifiedTableMappingCtrl> unifiedTableMappings = ctrls.Cast<UnifiedTableMappingCtrl>().ToList();
        if (unifiedTableMappings is not { Count: > 0 })
            return null;

        // INpOnWrapperResult is table results (not inherit from BaseCtrl)
        Func<TableCtrl, List<UnifiedTableMappingCtrl>?, Task<INpOnWrapperResult?>> getDataFunc =
            async (table, groupedMappings) =>
            {
                if (groupedMappings is not { Count: > 0 })
                    throw new InvalidOperationException($"{table.TableName}: Grouped mappings is not have any value.");
                if (table.ConnectionInfo == null)
                    throw new InvalidOperationException($"{table.TableName}: Connection info cannot be null.");
                IDbFactoryWrapper? dbFactoryWrapper =
                    InitializationCtrlSystem.CreateDbFactoryWrapper(table.ConnectionInfo).Result;
                string queryWithFieldAndTableObject = BaseQueryWithFieldAndTable.CreateQuery(groupedMappings);
                INpOnWrapperResult? getDataResult =
                    dbFactoryWrapper?.QueryAsync(queryWithFieldAndTableObject).GetAwaiter().GetResult();

                return null;
            };

        List<INpOnWrapperResult>? results = await JoiningTable(unifiedTableMappings, getDataFunc!);

        //
        INpOnSuperTableWrapper superTable = new NpOnSuperTableWrapper(results);
        return superTable;
    }

    private static async Task<List<INpOnWrapperResult>?> JoiningTable(
        List<UnifiedTableMappingCtrl> unifiedTableMappings,
        Func<TableCtrl, List<UnifiedTableMappingCtrl>?, Task<INpOnWrapperResult>>? getBulkDataMethod
    )
    {
        if (getBulkDataMethod == null)
            return null;
        List<(TableCtrl? Table, List<UnifiedTableMappingCtrl> MappingSorteds)> groupedByTable =
            unifiedTableMappings.ValidateAndSort();
        if (groupedByTable is not { Count: > 0 })
            return null;

        // get data async with each table group
        var tasks = groupedByTable
            .Select((grouped, index) => new
            {
                Index = index,
                Table = grouped.Table!,
                Task = WrapperProcessers.Processer(getBulkDataMethod!, grouped.Table!, grouped.MappingSorteds)
            })
            .ToList();

        var resultsArray = await Task.WhenAll(tasks.Select(t => t.Task));

        List<(TableCtrl Table, INpOnWrapperResult WrapperResult)> results = tasks
            .Zip(resultsArray, (t, rs) => new { t.Index, t.Table, Result = rs })
            .Where(x => x.Result != null)
            .OrderBy(x => x.Index) // order by to sort same as input index for loop
            .Select(x => (x.Table, x.Result!))
            .ToList();

        // todo: Join with EDbJoinType to Create SuperTable from table
        return null;
    }


    /// <summary>
    /// Call this func when object build done from database
    /// </summary>
    /// <param name="tableMappings"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static List<(TableCtrl? Table, List<UnifiedTableMappingCtrl> MappingSorteds)> ValidateAndSort(
        this List<UnifiedTableMappingCtrl>? tableMappings)
    {
        if (tableMappings is not { Count: > 0 })
            return [];

        // validate 
        HashSet<string> fieldNames = [];
        foreach (var tableMapping in tableMappings)
        {
            EDbJoinType? joinType = tableMapping.JoinType == DefaultValueForObject.DefaultValueForEnumInt
                ? null
                : tableMapping.JoinType;
            long? joinTableFieldId = tableMapping.JoinTableFieldId == DefaultValueForObject.DefaultValueForInt
                ? null
                : tableMapping.JoinTableFieldId;
            TableFieldCtrl? joinTableField = tableMapping.JoinTableField;
            int? joinOrder = tableMapping.JoinOrder == DefaultValueForObject.DefaultValueForInt
                ? null
                : tableMapping.JoinOrder;
            ;

            // Validate
            // Check if all four are null or all four are non-null
            bool allNull = joinType == null && joinTableFieldId == null && joinTableField == null && joinOrder == null;
            bool allNotNull = joinType != null && joinTableFieldId != null && joinTableField != null &&
                              joinOrder != null;

            if (!(allNull || allNotNull))
                throw new InvalidOperationException(
                    "JoinType, JoinTableFieldId, JoinTableField, and JoinOrder must either all be null or all be non-null.");
            if (allNull)
                return [];
            if (tableMapping.UnifiedTable == null ||
                tableMapping.TableField == null ||
                tableMapping.TableField.Table == null
               )
                return [];
            fieldNames.Add(tableMapping.TableField.FieldName);
        }

        if (fieldNames.Count != tableMappings.Count)
            throw new InvalidOperationException(
                "All TableField.FieldName values must be unique within the tableMappings list.");

        // group (sort ascending - null as infinity)
        List<(TableCtrl? Table, List<UnifiedTableMappingCtrl> MappingSorteds)> groupedByTable =
            tableMappings
                .Where(m => m.TableField?.Table != null)
                .GroupBy(
                    m => m.TableField!.Table!.Id,
                    (_, g) =>
                    {
                        var mappings = g.ToList();
                        var sortedMappings = mappings
                            .OrderBy(m => m.JoinOrder == null)
                            .ThenBy(m => m.JoinOrder)
                            .ToList();

                        return (
                            mappings.First().TableField!.Table,
                            Mappings: sortedMappings
                        );
                    })
                .ToList();

        if (groupedByTable.Count == 0)
            return [];

        return groupedByTable;
    }
}