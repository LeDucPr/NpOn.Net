using CommonDb.DbResults;
using HandlerFlow.AlgObjs.CtrlObjs;
using PostgresExtCm.Results;

namespace SystemController.ResultConverters;

public static class ResultConverter
{
    public static BaseCtrl? Converter(this INpOnWrapperResult result, Type ctrlType)
    {
        if (!ctrlType.IsChildOfBaseCtrl())
            return null;

        if (result is not PostgresResultSetWrapper pgResult)
            return null;

        string[] columnNames = pgResult.Columns.Keys.ToArray();
        if (columnNames.Length == 0)
            return null;

        var emptyCtrl = (BaseCtrl?)Activator.CreateInstance(ctrlType);
        
        var fieldMapProp = typeof(BaseCtrl).GetProperty(
            nameof(BaseCtrl.FieldMap),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var fieldMap = fieldMapProp?.GetValue(emptyCtrl) as Dictionary<string, string>;
        if (fieldMap is null || fieldMap.Count == 0)
            return emptyCtrl;
        
        foreach (var prop in ctrlType.GetProperties(System.Reflection.BindingFlags.Public |
                                                    System.Reflection.BindingFlags.Instance))
        {
            if (!prop.CanWrite)
                continue;
            if (!fieldMap.TryGetValue(prop.Name, out var columnName) || string.IsNullOrWhiteSpace(columnName))
                continue;

            if (!pgResult.Columns.ContainsKey(columnName))
                continue;

            var value = pgResult.Rows[0].Result[columnName];

            
        }

        
        
        
        

        // var ctrl = (BaseCtrl?)Activator.CreateInstance(ctrlType);
        // if (ctrl == null)
        //     return null;
        //
        // // Ensure FieldMap is populated by invoking FieldMapper via nameof
        // var fieldMapper = ctrlType.GetMethod(
        //     nameof(BaseCtrl.FieldMap),
        //     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        // fieldMapper?.Invoke(ctrl, null);
        //
        // // Access FieldMap via nameof to avoid magic strings
        // var fieldMapProp = typeof(BaseCtrl).GetProperty(
        //     nameof(BaseCtrl.FieldMap),
        //     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        // var fieldMap = fieldMapProp?.GetValue(ctrl) as Dictionary<string, string>;
        // if (fieldMap is null || fieldMap.Count == 0)
        //     return ctrl;

        // Columns snapshot

        
        
        //
        // foreach (var prop in ctrlType.GetProperties(System.Reflection.BindingFlags.Public |
        //                                             System.Reflection.BindingFlags.Instance))
        // {
        //     if (!prop.CanWrite)
        //         continue;
        //
        //     if (!fieldMap.TryGetValue(prop.Name, out var columnName) || string.IsNullOrWhiteSpace(columnName))
        //         continue;
        //
        //     if (!pgResult.Columns.ContainsKey(columnName))
        //         continue;
        //
        //     var value = pgResult.Rows[0].Result[columnName];
        //
        //     try
        //     {
        //         // var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        //         //
        //         // // Handle enums explicitly
        //         // object? convertedValue;
        //         // if (targetType.IsEnum)
        //         // {
        //         //     convertedValue = value is string s
        //         //         ? Enum.Parse(targetType, s, ignoreCase: true)
        //         //         : Enum.ToObject(targetType, Convert.ChangeType(value, Enum.GetUnderlyingType(targetType)));
        //         // }
        //         // else
        //         // {
        //         //     convertedValue = Convert.ChangeType(value, targetType);
        //         // }
        //         //
        //         // prop.SetValue(ctrl, convertedValue);
        //     }
        //     catch
        //     {
        //         // Ignore conversion errors; optionally log here
        //     }
        // }

        return emptyCtrl;
    }


    // public void ToRow(PostgresResultSetWrapper)
    // {
    //     PostgresResultSetWrapper 
    // }
}