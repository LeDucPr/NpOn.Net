using System.Collections.Concurrent;
using System.Reflection;
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

    // cache data (call methods) JoinList (contains session id call) 
    private static readonly ConcurrentDictionary<string /*sessionId*/, JoinListLookup> MetadataBaseCtrlCache = new();


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


    #region Calling Method Id

    public static string CreateCallingMethodSessionIdc()
    {
        var st = new System.Diagnostics.StackTrace();
        var sf = st.GetFrame(1);
        var method = sf?.GetMethod();
        return $"{method?.DeclaringType?.FullName}.{method?.Name}_{System.Guid.NewGuid()}";
    }
    
    public static void AddToLookupData(this string sessionId, DataLookup dataLookup)
    {
        MetadataBaseCtrlCache.AddOrUpdate(
            sessionId,
            _ => // if not exist key => create new JoinListLookup
            {
                var lookup = new JoinListLookup(sessionId);
                lookup.Merge(dataLookup);
                return lookup;
            },
            (_, existingLookup) => // else => merge into JoinListLookup
            {
                existingLookup.Merge(dataLookup);
                return existingLookup;
            });
    }

    #endregion Calling Method Id
}

public static partial class RaisingIndexer
{
    public static async Task JoinList(
        this BaseCtrl? ctrl,
        Func<Type, Task<string>>? createStringQueryMethod,
        Func<string, Task<BaseCtrl>>? getDataMethod,
        int recursionStopLoss = -1, // max size of recursion loop (-1 == unlimited)
        int currentRecursionLoop = 1)
    {
        if (createStringQueryMethod == null || getDataMethod == null)
            return;
        if (!IsTableLoaderAttached(ctrl))
            return;

        // validate primary keys (value)
        var primaryKeys = ctrl.PrimaryKeys()?.ToList();
        if (primaryKeys is not { Count: > 0 } ||
            primaryKeys.Any(key => key.Property.GetValue(ctrl) == null))
            return;

        // validate foreign keys (relationship)
        KeyInfo[]? fks = ctrl.ForeignKeys()?.ToArray();
        if (fks is not { Length: > 0 })
            return;
        KeyInfo[]? fkIds = ctrl.ForeignKeyIds()?.ToArray();
        if (fkIds is not { Length: > 0 })
            return;

        // validate attribute foreign keys and foreign key ids
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

        if (recursionStopLoss >= 0 && recursionStopLoss < currentRecursionLoop)
            return;

        foreach (Type? fkType in fkTypes) // foreign keys
        {
            if (fkType == null)
                continue;

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

            await JoinList(ctrlFromKey, createStringQueryMethod, getDataMethod,
                recursionStopLoss, ++currentRecursionLoop); // recursions 
        }
    }
}