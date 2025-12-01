using System.Reflection;
using CommonDb.DbResults.Grpc;

namespace SSO.ServiceModels;

public abstract class BaseSsoNpOnGrpcModel
{
    #region Field Config

    // field mapper (initializer)
    public virtual Dictionary<string, string>? FieldMap { get; protected set; }

    /// <summary>
    /// call in first requisition 
    /// </summary>
    public void CreateDefaultFieldMapper()
    {
        if (FieldMap is { Count: > 0 })
            throw new ArgumentNullException(nameof(FieldMap) + "is created");
        BaseFieldMapper();
    }

    private void BaseFieldMapper()
    {
        FieldMap ??= new();
        FieldMapper();
    }

    #endregion Field Config

    protected abstract void FieldMapper();
}

public static class BaseSsoNpOnGrpcModelExtensions
{
    public static IEnumerable<BaseSsoNpOnGrpcModel>? ConverterToChildOfSsoModel(this INpOnGrpcObject result, Type ssoNpOnGrpcModelType)
    {
        if (!ssoNpOnGrpcModelType.IsSubclassOf(typeof(BaseSsoNpOnGrpcModel)))
            return null;

        if (result is not NpOnGrpcTable grpcTable)
            return null;

        if (grpcTable.Rows is not { Count: > 0 })
            return null;

        var emptyCtrl = (BaseSsoNpOnGrpcModel?)Activator.CreateInstance(ssoNpOnGrpcModelType);
        if (emptyCtrl == null)
            return null;

        emptyCtrl.CreateDefaultFieldMapper();
        if (emptyCtrl.FieldMap is not { Count: > 0 })
            return null;

        var properties = ssoNpOnGrpcModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p);

        List<BaseSsoNpOnGrpcModel> modelList = new();
        foreach (var rowKvp in grpcTable.Rows)
        {
            var newModel = (BaseSsoNpOnGrpcModel?)Activator.CreateInstance(ssoNpOnGrpcModelType);
            if (newModel == null)
                continue;

            foreach (var mapKvp in emptyCtrl.FieldMap)
            {
                string propertyName = mapKvp.Key;
                string columnName = mapKvp.Value;

                if (properties.TryGetValue(propertyName, out var propInfo) &&
                    rowKvp.Value.Cells.TryGetValue(columnName, out var cell) &&
                    cell is { ValueBytes: not null })
                {
                    var actualType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;
                    object? valueFromCell = cell.ValueAsObject;

                    if (valueFromCell != null)
                    {
                        // The value from ValueAsObject is already in the correct type (or a compatible one).
                        // We don't need Convert.ChangeType, which fails for non-IConvertible types like Guid.
                        // For enums, ValueAsObject returns the underlying integral type (long), so we need to convert it.
                        if (actualType.IsEnum)
                            propInfo.SetValue(newModel, Enum.ToObject(actualType, valueFromCell));
                        else
                            propInfo.SetValue(newModel, valueFromCell);
                    }
                }
            }
            modelList.Add(newModel);
        }
        return modelList;
    }
}