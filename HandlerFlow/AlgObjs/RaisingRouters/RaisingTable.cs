using Enums;
using HandlerFlow.AlgObjs.CtrlObjs.Data;

namespace HandlerFlow.AlgObjs.RaisingRouters;

public static class RaisingTable
{
    public static bool UnifiedTableValidate(List<UnifiedTableMappingCtrl>? tableMappings)
    {
        if (tableMappings is not { Count: > 0 })
            return false;

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
                return false;
            if (tableMapping.UnifiedTable == null ||
                tableMapping.TableField == null ||
                tableMapping.TableField.Table == null
               )
                return false;
            fieldNames.Add(tableMapping.TableField.FieldName);
        }

        if (fieldNames.Count != tableMappings.Count)
            throw new InvalidOperationException(
                "All TableField.FieldName values must be unique within the tableMappings list.");

        return true;
    }
}