using ProtoBuf;

namespace Common.Extensions.NpOn.HandleFlow;

[ProtoContract]
public abstract class BaseCtrl
{
    #region Field Config

    // field mapper (initializer)
    public abstract Dictionary<string, string>? FieldMap { get; protected set; }

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

    public void FieldMapperCustom(Dictionary<string, string> newFieldMap, bool isOverrideMapperFieldInfo = true)
    {
        if (isOverrideMapperFieldInfo)
            FieldMap = new Dictionary<string, string>();
        newFieldMap.ToList().ForEach(x => FieldMap?.Add(x.Key, x.Value));
    }

    #endregion Field Config

    protected abstract void FieldMapper();
}

public static class BaseCtrlExtensions
{
    /// <summary>
    /// Is Inherit class from BaseCtrl?
    /// </summary>
    /// <param name="ctrlType"></param>
    /// <returns></returns>
    public static bool IsChildOfBaseCtrl(this Type ctrlType)
    {
        Type baseType = typeof(BaseCtrl);
        return ctrlType != baseType && ctrlType.IsSubclassOf(baseType);
    }

    /// <summary>
    /// used in caching when retrieving the first object,
    /// the parameter from FieldMap will be loaded into the cache and reused for objects of the same Type
    /// </summary>
    /// <param name="ctrlType"></param>
    /// <returns></returns>
    public static BaseCtrl? CreateDefaultFieldMapperWithEmptyBaseCtrlObject(this Type ctrlType)
    {
        var emptyCtrl = (BaseCtrl?)Activator.CreateInstance(ctrlType);
        emptyCtrl?.CreateDefaultFieldMapper();
        return emptyCtrl;
    }
}