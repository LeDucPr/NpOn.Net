using ObjectHandlerFlow.AlgObjs.Attributes;

namespace ObjectHandlerFlow.AlgObjs.CtrlObjs;

public abstract class SysBaseCtrl : BaseCtrl
{
    #region Field Config

    [Pk(nameof(SysBaseCtrl.Id))] public required long Id { get; set; }

    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public override Dictionary<string, string>? FieldMap { get; protected set; }
    
    protected override void FieldMapper()
    {
        FieldMap ??= new();
        FieldMap.Add(nameof(Id), "id");
        FieldMap.Add(nameof(CreatedAt), "created_at");
        FieldMap.Add(nameof(UpdatedAt), "updated_at");
        FieldMap.Add(nameof(Description), "description");
        FieldMap.Add(nameof(IsActive), "is_active");
    }

    #endregion Field Config
}

public static class SysBaseCtrlExtensions
{
    /// <summary>
    /// Is Inherit class from BaseCtrl?
    /// </summary>
    /// <param name="ctrlType"></param>
    /// <returns></returns>
    public static bool IsChildOfSysBaseCtrl(this Type ctrlType)
    {
        Type baseType = typeof(SysBaseCtrl);
        return ctrlType != baseType && ctrlType.IsSubclassOf(baseType);
    }

    /// <summary>
    /// used in caching when retrieving the first object,
    /// the parameter from FieldMap will be loaded into the cache and reused for objects of the same Type
    /// </summary>
    /// <param name="ctrlType"></param>
    /// <returns></returns>
    public static SysBaseCtrl? CreateDefaultFieldMapperWithEmptySysBaseCtrlObject(this Type ctrlType)
    {
        var emptyCtrl = (SysBaseCtrl?)Activator.CreateInstance(ctrlType);
        emptyCtrl?.CreateDefaultFieldMapper();
        return emptyCtrl;
    }
}