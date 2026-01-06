using System.Reflection;
using Common.Infrastructures.NpOn.CommonDb.DbResults.Grpc;

namespace Common.Infrastructures.NpOn.CommonDb.DbResults;

public static class NpOnWrapperResultExtensions
{
    #region Private Helpers

    private static bool TryGetConversionContext<T>(
        INpOnWrapperResult? wrapperResult,
        out IReadOnlyDictionary<int, INpOnRowWrapper?>? rowWrappers,
        out Dictionary<string, string>? fieldMap,
        out Dictionary<string, PropertyInfo>? properties)
        where T : NpOnBaseGrpcObject
    {
        fieldMap = null;
        properties = null;
        rowWrappers = null;

        if (wrapperResult is not INpOnTableWrapper tableWrapper || tableWrapper.RowWrappers is not { Count: > 0 })
        {
            return false;
        }

        rowWrappers = tableWrapper.RowWrappers;
        
        var tempInstance = Activator.CreateInstance<T>();
        // This is the crucial fix: Initialize the FieldMap before trying to access it.
        tempInstance.CreateDefaultFieldMapper();
        var fieldMapProp = typeof(T).GetProperty(nameof(NpOnBaseGrpcObject.FieldMap)); 

        if (tempInstance.FieldMap is { Count: > 0 } map)
        {
            fieldMap = map;
        }
        else // Fallback: create default map property -> property
        {
            fieldMap = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .ToDictionary(p => p.Name, p => p.Name);
        }

        if (fieldMap is not { Count: > 0 })
            return false;

        properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p);

        return properties.Count > 0;
    }

    private static T ConvertRowToObject<T>(
        T instance,
        INpOnRowWrapper rowWrapper,
        IReadOnlyDictionary<string, string> fieldMap,
        IReadOnlyDictionary<string, PropertyInfo> properties)
        where T : NpOnBaseGrpcObject
    {
        var cells = rowWrapper.GetRowWrapper();
        foreach (var (propertyName, columnName) in fieldMap)
        {
            if (properties.TryGetValue(propertyName, out var propInfo) &&
                cells.TryGetValue(columnName, out var cell) &&
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
                    // todo: ignore
                }
            }
        }
        return instance;
    }

    #endregion

    #region Public Extensions

    public static T? ToFirstOrDefault<T>(this INpOnWrapperResult? result) where T : NpOnBaseGrpcObject
    {
        if (!TryGetConversionContext<T>(result, out var rowWrappers, out var fieldMap, out var properties))
            return default;

        var firstRow = rowWrappers!.Values.FirstOrDefault(r => r != null);
        if (firstRow == null)
            return default;

        var instance = Activator.CreateInstance<T>();
        return ConvertRowToObject(instance, firstRow, fieldMap!, properties!);
    }

    public static T? ToLastOrDefault<T>(this INpOnWrapperResult? result) where T : NpOnBaseGrpcObject
    {
        if (!TryGetConversionContext<T>(result, out var rowWrappers, out var fieldMap, out var properties))
            return default;

        var lastRow = rowWrappers!.Values.LastOrDefault(r => r != null);
        if (lastRow == null)
            return default;

        var instance = Activator.CreateInstance<T>();
        return ConvertRowToObject(instance, lastRow, fieldMap!, properties!);
    }

    public static List<T>? ToList<T>(this INpOnWrapperResult? result) where T : NpOnBaseGrpcObject
    {
        if (!TryGetConversionContext<T>(result, out var rowWrappers, out var fieldMap, out var properties))
            return null;

        var list = new List<T>(rowWrappers!.Count);
        foreach (var row in rowWrappers.Values)
        {
            if (row == null) continue;
            var instance = Activator.CreateInstance<T>();
            list.Add(ConvertRowToObject(instance, row, fieldMap!, properties!));
        }

        return list.Count > 0 ? list : null;
    }

    public static List<T>? ToRange<T>(this INpOnWrapperResult? result, int skip, int take) where T : NpOnBaseGrpcObject
    {
        if (skip < 0 || take <= 0)
            return null;

        if (!TryGetConversionContext<T>(result, out var rowWrappers, out var fieldMap, out var properties))
            return null;

        var list = new List<T>(take);
        foreach (var row in rowWrappers!.Values.Where(r => r != null).Skip(skip).Take(take))
        {
            var instance = Activator.CreateInstance<T>();
            if (row != null) list.Add(ConvertRowToObject(instance, row, fieldMap!, properties!));
        }

        return list.Count > 0 ? list : null;
    }

    #endregion
}