using System.Reflection;

namespace Common.Infrastructures.NpOn.CommonDb.DbResults.Grpc;

public static class NpOnBaseGrpcObjectExtensions
{
    #region private

    private static T ConvertRowToObject<T>(
        T instance,
        NpOnGrpcRow row,
        IReadOnlyDictionary<string, string> fieldMap,
        IReadOnlyDictionary<string, PropertyInfo> properties)
        where T : NpOnBaseGrpcObject
    {
        foreach (var (propertyName, columnName) in fieldMap)
        {
            if (properties.TryGetValue(propertyName, out var propInfo) &&
                row.Cells.TryGetValue(columnName, out var cell) &&
                cell.ValueAsObject is { } value)
            {
                var targetType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;

                try
                {
                    if (targetType.IsEnum)
                        propInfo.SetValue(instance, Enum.ToObject(targetType, value));
                    else
                        propInfo.SetValue(instance, Convert.ChangeType(value, targetType), null);
                }
                catch
                {
                    // Ignore conversion errors
                }
            }
        }

        return instance;
    }

    private static bool TryGetConversionContext<T>(
        INpOnGrpcObject? grpcObject,
        out NpOnGrpcTable? grpcTable,
        out Dictionary<string, string>? fieldMap,
        out Dictionary<string, PropertyInfo>? properties)
        where T : NpOnBaseGrpcObject
    {
        fieldMap = null;
        properties = null;
        if (grpcObject == null)
        {
            grpcTable = null;
            return false;
        }

        grpcTable = grpcObject as NpOnGrpcTable;

        if (grpcTable?.Rows is not { Count: > 0 })
            return false;

        // create instance to get FieldMap
        var tempInstance = Activator.CreateInstance<T?>();
        if (tempInstance == null)
            return false;

        tempInstance.CreateDefaultFieldMapper();
        fieldMap = tempInstance.FieldMap;

        if (fieldMap is not { Count: > 0 })
            return false;

        properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p);

        return properties.Count > 0;
    }

    #endregion private


    public static T? ToFirstOrDefault<T>(this INpOnGrpcObject result)
        where T : NpOnBaseGrpcObject
    {
        if (!TryGetConversionContext<T>(result, out var grpcTable, out var fieldMap, out var properties))
            return null;

        var firstRow = grpcTable?.Rows?.Values.FirstOrDefault();
        if (firstRow == null)
            return null;

        var instance = Activator.CreateInstance<T>();
        return ConvertRowToObject(instance, firstRow, fieldMap!, properties!);
    }

    public static T? ToLastOrDefault<T>(this INpOnGrpcObject result)
        where T : NpOnBaseGrpcObject
    {
        if (!TryGetConversionContext<T>(result, out var grpcTable, out var fieldMap, out var properties))
            return null;

        var lastRow = grpcTable?.Rows?.Values.LastOrDefault();
        if (lastRow == null)
            return null;

        var instance = Activator.CreateInstance<T>();
        return ConvertRowToObject(instance, lastRow, fieldMap!, properties!);
    }

    public static List<T>? ToList<T>(this INpOnGrpcObject? result)
        where T : NpOnBaseGrpcObject
    {
        if (!TryGetConversionContext<T>(result, out var grpcTable, out var fieldMap, out var properties))
            return null;

        var firstRow = grpcTable?.Rows?.Values.FirstOrDefault();
        if (firstRow == null)
            return null;

        var list = new List<T>(grpcTable!.Rows!.Count);

        foreach (var row in grpcTable.Rows.Values)
        {
            var instance = Activator.CreateInstance<T>();
            list.Add(ConvertRowToObject(instance, row, fieldMap!, properties!));
        }

        return list;
    }

    public static List<T>? ToRange<T>(this INpOnGrpcObject? result, int skip, int take)
        where T : NpOnBaseGrpcObject
    {
        if (skip < 0 || take <= 0)
            return null;

        if (!TryGetConversionContext<T>(result, out var grpcTable, out var fieldMap, out var properties))
            return null;

        var firstRow = grpcTable?.Rows?.Values.FirstOrDefault();
        if (firstRow == null)
            return null;

        var rows = grpcTable!.Rows!.Values.Skip(skip).Take(take);
        var list = new List<T>();

        foreach (var row in rows)
        {
            var instance = Activator.CreateInstance<T>();
            list.Add(ConvertRowToObject(instance, row, fieldMap!, properties!));
        }

        return list;
    }

    public static IEnumerable<NpOnBaseGrpcObject>? ConverterToChildOfBaseGrpcObject(
        this INpOnGrpcObject? result,
        Type modelType)
    {
        if (!modelType.IsSubclassOf(typeof(NpOnBaseGrpcObject)))
            return null;

        var method = typeof(NpOnBaseGrpcObjectExtensions)
            .GetMethod(nameof(ToList), BindingFlags.Public | BindingFlags.Static);

        if (method == null)
            return null;

        var genericMethod = method.MakeGenericMethod(modelType);
        var invokeResult = genericMethod.Invoke(null, [result]);

        return invokeResult as IEnumerable<NpOnBaseGrpcObject>;
    }
}