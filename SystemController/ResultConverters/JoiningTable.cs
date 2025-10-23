using CommonDb.DbResults;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs.Data;
using System.Data;

namespace SystemController.ResultConverters;

public static class JoiningTable
{
    public async static Task LoadData(
        // Task<> /////////////////
        )
    {
        
    }
    
    
    /// <summary>
    /// Call this func when object build done from database
    /// </summary>
    /// <param name="tableMappings"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static List<(TableCtrl? Table, List<UnifiedTableMappingCtrl> Mappings)> ValidateAndSort(this List<UnifiedTableMappingCtrl>? tableMappings)
    {
        if (tableMappings is not { Count: > 0 })
            return [];

        // validate 
        HashSet<string> fieldNames = [];
        foreach (var tableMapping in tableMappings)
        {
            EDbJoinType? joinType = tableMapping.JoinType;
            long? joinTableFieldId = tableMapping.JoinTableFieldId;
            TableFieldCtrl? joinTableField = tableMapping.JoinTableField;
            int? joinOrder = tableMapping.JoinOrder;

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
        List<(TableCtrl? Table, List<UnifiedTableMappingCtrl> Mappings)> groupedByTable =
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

        foreach (var grouped in groupedByTable)
        {
            //
            //
            // todo: build  query with group field with table name call in key (TableCtrl) - with field in table of database
            //
            //
            
            TableCtrl? table = grouped.Table;
            if (table == null) // decoy data without null
                return [];
            List<UnifiedTableMappingCtrl> tableFieldMappings = grouped.Mappings;
            if (tableFieldMappings is not { Count: > 0 })
                return [];
        }

        return groupedByTable;
    }
}