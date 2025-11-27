using System.Reflection;
using AccountServiceObject.BusinessObjects;
using CommonDb.DbResults.Grpc;
using ProtoBuf;

namespace AccountServiceObject;

[ProtoInclude(100, typeof(AccountLoginInfoObject))]
public abstract class BaseAccountObjectFromGrpcTable
{
    #region Field Config

    public virtual Dictionary<string, string>? FieldMap { get; protected set; }

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
    public static IEnumerable<BaseAccountObjectFromGrpcTable>? ConverterToChildOfBaseAccountObjectFromGrpcTable(
        this INpOnGrpcObject result, Type ssoNpOnGrpcModelType)
    {
        if (!ssoNpOnGrpcModelType.IsSubclassOf(typeof(BaseAccountObjectFromGrpcTable)))
            return null;

        if (result is not NpOnGrpcTable grpcTable)
            return null;

        if (grpcTable.Rows is not { Count: > 0 })
            return null;

        var emptyCtrl = (BaseAccountObjectFromGrpcTable?)Activator.CreateInstance(ssoNpOnGrpcModelType);
        if (emptyCtrl == null)
            return null;

        emptyCtrl.CreateDefaultFieldMapper();
        if (emptyCtrl.FieldMap is not { Count: > 0 })
            return null;

        var properties = ssoNpOnGrpcModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p);

        List<BaseAccountObjectFromGrpcTable> modelList = new();
        foreach (var rowKvp in grpcTable.Rows)
        {
            var newModel = (BaseAccountObjectFromGrpcTable?)Activator.CreateInstance(ssoNpOnGrpcModelType);
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