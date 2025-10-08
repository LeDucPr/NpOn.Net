using System.Collections.Concurrent;
using HandlerFlow.AlgObjs.Attributes;
using HandlerFlow.AlgObjs.CtrlObjs;
using CommonMode;

namespace HandlerFlow.AlgObjs.RaisingRouters;

public static partial class RaisingIndexer
{
    // Caching struct
    private static readonly ConcurrentDictionary<Type, KeyMetadataInfo> MetadataCache = new();
    private static readonly ConcurrentDictionary<Type, bool> EnableObjectCache = new();

    #region Cache

    private static KeyMetadataInfo GetOrScanTypeMetadata(Type type)
    {
        return MetadataCache.GetOrAdd(type, t =>
        {
            // PkAttribute (generic + non-generic)
            var pkInfos = t.GetPropertiesWithAttribute<PkAttribute>()
                .Select(p => new KeyInfo(p.propertyInfo, p.attribute))
                .ToList();
            pkInfos.AddRange(t.GetPropertiesWithGenericAttribute(typeof(PkAttribute<>))
                .Select(p => new KeyInfo(p.propertyInfo, p.attribute))
                .ToList());
            // FkAttribute (generic)
            var fkInfos = t.GetPropertiesWithGenericAttribute(typeof(FkAttribute<>))
                .Select(p => new KeyInfo(p.propertyInfo, p.attribute))
                .ToList();
            // FkIdAttribute
            var fkIdInfos = t.GetPropertiesWithGenericAttribute(typeof(FkIdAttribute<>))
                .Select(p => new KeyInfo(p.propertyInfo, p.attribute))
                .ToList();
            return new KeyMetadataInfo(pkInfos, fkInfos, fkIdInfos); // cache
        });
    }

    private static bool GetOrScanTypeEnableObjectCache(Type? type)
    {
        if (type == null) return false;
        return EnableObjectCache.GetOrAdd(type, _ => AttributeMode.HasClassAttribute<TableLoaderAttribute>(type));
    }

    #endregion Cache


    #region Public Methods

    /// <summary>
    /// For stater
    /// </summary>
    /// <param name="ctrl"></param>
    /// <returns></returns>
    public static BaseCtrl? AnalyzeAndDisplayKeys(this BaseCtrl? ctrl)
    {
        if (ctrl == null)
            return null;
        var metadata = GetOrScanTypeMetadata(ctrl.GetType()); // First call may be slow
        if (metadata.PrimaryKeys.Count == 0)
            return null;
        return ctrl;
    }

    public static KeyMetadataInfo? KeyMetadata(this BaseCtrl? ctrl)
    {
        if (ctrl == null)
            return null;
        var metadata = GetOrScanTypeMetadata(ctrl.GetType());
        return metadata;
    }

    public static IEnumerable<KeyInfo>? PrimaryKeys(this BaseCtrl? ctrl)
    {
        if (ctrl == null)
            return null;
        var pks = GetOrScanTypeMetadata(ctrl.GetType()).PrimaryKeys;
        return pks;
    }

    public static IEnumerable<KeyInfo>? ForeignKeys(this BaseCtrl? ctrl)
    {
        if (ctrl == null)
            return null;
        var fks = GetOrScanTypeMetadata(ctrl.GetType()).ForeignKeys;
        return fks;
    }

    public static IEnumerable<KeyInfo>? ForeignKeyIds(this BaseCtrl? ctrl)
    {
        if (ctrl == null)
            return null;
        var fkIds = GetOrScanTypeMetadata(ctrl.GetType()).ForeignKeyIds;
        return fkIds;
    }

    public static bool IsTableLoaderAttached(this BaseCtrl? ctrl)
        => GetOrScanTypeEnableObjectCache(ctrl?.GetType());

    #endregion Public Methods
}

public static partial class RaisingIndexer
{
    public static void JoinList(this BaseCtrl? ctrl)
    {
        if (!IsTableLoaderAttached(ctrl))
            return;

        KeyInfo[]? fks = ctrl.ForeignKeys()?.ToArray();
        if (fks is not { Length: > 0 })
            return;
        KeyInfo[]? fkIds = ctrl.ForeignKeyIds()?.ToArray();
        if (fkIds is not { Length: > 0 })
            return;

        // validate foreign keys and foreign key ids
        Attribute[] fkAttrs = fks.Select(x => x.Attribute).ToArray();
        Type fkAttrType = fkAttrs.First().GetType();
        if (!fkAttrType.IsGenericType || fkAttrType.GetGenericTypeDefinition() != typeof(FkAttribute<>))
        {
            return;
        }

        Attribute[] fkIdAttrs = fkIds.Select(x => x.Attribute).ToArray();
        Type fkIdAttrType = fkIdAttrs.First().GetType();
        if (fkIdAttrType.GetGenericTypeDefinition() != typeof(FkIdAttribute<>))
        {
            return;
        }

        Type?[] fkTypes = fkAttrs.Select(x => x.GetPropertyTypeFromAttribute(nameof(FkAttribute<>.RelatedType)))
            .ToArray();
        Type?[] fkIdsTypes = fkAttrs.Select(x => x.GetPropertyTypeFromAttribute(nameof(FkIdAttribute<>.RelatedType)))
            .ToArray();

        fkTypes = fkTypes.Where(x => fkIdsTypes.Contains(x)).ToArray();
        fkIdsTypes = fkIdsTypes.Where(x => fkTypes.Contains(x)).ToArray();

        if (fkIds.Length == 0 || fkIds.Length != fks.Length)
            return;

        // fks.GetOrScanTypeMetadata()
        foreach (Type? fkType in fkTypes)
        {
            if (fkType == null)
                continue;
            // Todo
            // truy vấn bảng với khóa từ fkIds với các ngoại
            // cần thêm mod để ngắt truy vấn liên tục bằng đệ quy
            if (GetOrScanTypeMetadata(fkType).ForeignKeys.Count == 0)
            {
                return;
            }
        }

        // KeyInfo? pk = ctrl.PrimaryKeys()?.FirstOrDefault(x => x.Property.Name.ToLower().Contains(nameof(BaseCtrl.Id)));
    }
}