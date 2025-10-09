using System.Collections.Concurrent;
using HandlerFlow.AlgObjs.Attributes;
using HandlerFlow.AlgObjs.CtrlObjs;
using CommonMode;
using HandlerFlow.WrapperProcessors;

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
    public static async Task JoinList(
        this BaseCtrl? ctrl,
        Func<Type, Task<string>>? createStringQueryMethod,
        Func<string, Task<BaseCtrl>>? getDataMethod)
    {
        if (createStringQueryMethod == null || getDataMethod == null)
            return;
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
            return;
        
        Attribute[] fkIdAttrs = fkIds.Select(x => x.Attribute).ToArray();
        Type fkIdAttrType = fkIdAttrs.First().GetType();
        if (fkIdAttrType.GetGenericTypeDefinition() != typeof(FkIdAttribute<>))
            return;
        
        // need add both the FkId<T> and Fk<T> when create their relationship, or break 
        Type?[] fkTypes = fkAttrs.Select(x => x.GetPropertyTypeFromAttribute(nameof(FkAttribute<>.RelatedType)))
            .ToArray();
        Type?[] fkIdsTypes = fkAttrs.Select(x => x.GetPropertyTypeFromAttribute(nameof(FkIdAttribute<>.RelatedType)))
            .ToArray();

        fkTypes = fkTypes.Where(x => fkIdsTypes.Contains(x)).ToArray();
        fkIdsTypes = fkIdsTypes.Where(x => fkTypes.Contains(x)).ToArray();

        if (fkIds.Length == 0 || fkIds.Length != fks.Length)
            return;

        foreach (Type? fkType in fkTypes) // foreign keys
        {
            if (fkType == null)
                continue;
            if (GetOrScanTypeMetadata(fkType).ForeignKeys.Count == 0)
            {
                continue;
            }

            // value info
            var fkIdInfo = fkIds.FirstOrDefault(ki =>
            {
                var attr = ki.Attribute;
                var relatedType = attr.GetPropertyTypeFromAttribute(nameof(FkIdAttribute<>.RelatedType));
                return relatedType == fkType;
            });
            if (fkIdInfo == null)
                continue;

            var fkIdValue = fkIdInfo.Property.GetValue(ctrl);

            if (fkIdValue == null)
                continue;

            Type baseType = typeof(BaseCtrl);
            if (fkType == baseType || !fkType.IsSubclassOf(baseType))
                continue;

            // SqlQuery (get key)
            string? query = await WrapperProcessers.Processer<Type, string>(createStringQueryMethod!, fkType); //checked
            if (string.IsNullOrWhiteSpace(query))
                continue;
            var ctrlFromKey = await WrapperProcessers.Processer<string, BaseCtrl>(getDataMethod!, query); //checked
            if (ctrlFromKey != null)
            {
                // relationship field (with key)
                var navigationKeyInfo = fks.FirstOrDefault(fk =>
                    fk.Attribute.GetPropertyTypeFromAttribute(nameof(FkAttribute<>.RelatedType)) == fkType);
                if (navigationKeyInfo != null)
                    navigationKeyInfo.Property.SetValue(ctrl, ctrlFromKey);
            }
            await JoinList(ctrlFromKey, createStringQueryMethod, getDataMethod); // recursions 
        }

        // KeyInfo? pk = ctrl.PrimaryKeys()?.FirstOrDefault(x => x.Property.Name.ToLower().Contains(nameof(BaseCtrl.Id)));
    }
}