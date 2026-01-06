using Common.Extensions.NpOn.HandleFlow.Raising;
using Common.Infrastructures.NpOn.CommonDb.DbResults;

namespace Common.Extensions.NpOn.HandleFlow;

public static class ResultConverter
{
    public static IEnumerable<BaseCtrl>? GenericConverter(this INpOnWrapperResult result, Type ctrlType)
    {
        if (!ctrlType.IsChildOfBaseCtrl())
            return null;

        if (result is not INpOnTableWrapper tableWrapper)
            return null;

        string[] columnNames = tableWrapper.CollectionWrappers.Keys.ToArray();
        if (columnNames.Length == 0)
            return null;

        var emptyCtrl = (BaseCtrl?)Activator.CreateInstance(ctrlType);
        FieldInfo? mapField = emptyCtrl.MapperFieldInfo();
        if (mapField == null)
            return null;

        if (mapField.KeyProperties is not { Count: > 0 }) // is not has any mapper field/property
            return null;

        List<BaseCtrl> ctrlList = new();
        foreach (var row in tableWrapper.RowWrappers)
        {
            var newCtrl = (BaseCtrl?)Activator.CreateInstance(ctrlType);
            if (newCtrl == null)
                continue;
            foreach (var kvProp in mapField.KeyProperties)
            {
                INpOnCell? cell = null;
                bool isExist = row.Value?.GetRowWrapper().TryGetValue(kvProp.Key, out cell) ?? false;
                if (isExist && cell is { ValueType: not null })
                {
                    object? convertedValue = cell.ValueAsObject == null 
                        ? null 
                        : Convert.ChangeType(cell.ValueAsObject, Nullable.GetUnderlyingType(cell.ValueType) ?? cell.ValueType);
                    Type curType = kvProp.Value.PropertyType;
                    var actualType = Nullable.GetUnderlyingType(curType) ?? curType;
                    if (actualType.IsEnum)
                    {
                        // var enumValue = Enum.ToObject(actualType, convertedValue);
                        // kvProp.Value.SetValue(newCtrl, enumValue);
                        var underlying = Enum.GetUnderlyingType(actualType);
                        var numeric = Convert.ChangeType(convertedValue, underlying);
                        var enumValue = Enum.ToObject(actualType, numeric);
                        kvProp.Value.SetValue(newCtrl, enumValue);
                    }
                    else
                        kvProp.Value.SetValue(newCtrl, convertedValue, null);
                }
            }

            ctrlList.Add(newCtrl);
        }

        return ctrlList;
    }
}