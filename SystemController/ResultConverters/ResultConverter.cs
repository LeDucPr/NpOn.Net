using CommonDb.DbResults;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.RaisingRouters;
using PostgresExtCm.Results;

namespace SystemController.ResultConverters;

public static class ResultConverter
{
    public static IEnumerable<BaseCtrl>? Converter(this INpOnWrapperResult result, Type ctrlType)
    {
        if (!ctrlType.IsChildOfBaseCtrl())
            return null;

        if (result is not PostgresResultSetWrapper pgResult)
            return null;

        string[] columnNames = pgResult.Columns.Keys.ToArray();
        if (columnNames.Length == 0)
            return null;

        var emptyCtrl = (BaseCtrl?)Activator.CreateInstance(ctrlType);
        FieldInfo? mapField = emptyCtrl.MapperFieldInfo();
        if (mapField == null)
            return null;

        if (mapField.KeyProperties is not { Count: > 0 }) // is not has any mapper field/property
            return null;

        List<BaseCtrl> ctrlList = new();
        foreach (var row in pgResult.Rows)
        {
            var newCtrl = (BaseCtrl?)Activator.CreateInstance(ctrlType);
            if (newCtrl == null)
                continue;
            foreach (var kvProp in mapField.KeyProperties)
            {
                row.Value.Result.TryGetValue(kvProp.Key, out var cell);
                if (cell is { ValueType: not null })
                {
                    object convertedValue = Convert.ChangeType(cell.ValueAsObject, cell.ValueType)!;
                    kvProp.Value.SetValue(newCtrl, convertedValue, null);
                }
            }
            ctrlList.Add(newCtrl);
        }

        return ctrlList;
    }
}