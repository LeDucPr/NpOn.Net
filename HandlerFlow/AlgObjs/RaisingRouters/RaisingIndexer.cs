using System.Reflection;
using HandlerFlow.AlgObjs.Attributes;
using HandlerFlow.AlgObjs.CtrlObjs;
using CommonMode;
using CommonObject;
using Enums;

namespace HandlerFlow.AlgObjs.RaisingRouters;

public static partial class RaisingIndexer
{
    // Caching struct
    private static readonly WrapperCacheStore<Type, KeyMetadataInfo> MetadataCache = new();
    private static readonly WrapperCacheStore<Type, bool> EnableObjectCache = new();

    // cache data (call methods) JoinList (contains session id call) 
    private static readonly WrapperCacheStore<string /*sessionId*/, JoinListLookup> MetadataBaseCtrlCache = new();

    // Cache of field metadata – loaded from other methods into BaseCtrl
    private static readonly WrapperCacheStore<AdvancedTypeKey, FieldInfo> ProfiledFieldMapCache = new();


    #region Cache KeyInfo

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

    #endregion Cache KeyInfo


    #region Public Methods KeyInfo (Get Data from cache)

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

    #endregion Public Methods (Get Data from cache)


    #region Cache Lookup Data

    private static void AddToLookupData(this string sessionId, DataLookup dataLookup)
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

    // viết hàm lẫy dữ liệu từ lookup data
    public static JoinListLookup? GetLookupData(this string sessionId)
    {
        MetadataBaseCtrlCache.TryGetValue(sessionId, out var lookup);
        return lookup;
    }

    #endregion Cache Lookup Data


    #region Cache FieldMap

    private static FieldInfo GetOrScanFieldMap(Type ctrlType)
    {
        var emptyCtrl = (BaseCtrl?)Activator.CreateInstance(ctrlType);
        var fieldMap = emptyCtrl?.FieldMap ?? ctrlType.CreateDefaultFieldMapperWithEmptyObject()?.FieldMap;
        if (fieldMap == null || fieldMap.Count == 0)
            return new FieldInfo(new Dictionary<string, PropertyInfo>());
        // AdvancedTypeKey (unique with fieldMap + ctrlType)
        var advKey = new AdvancedTypeKey(fieldMap, ctrlType);
        // Caching
        return ProfiledFieldMapCache.GetOrAdd(advKey, _ =>
        {
            var keyProps = new Dictionary<string, PropertyInfo>();
            foreach (var kv in fieldMap)
            {
                var prop = ctrlType.GetProperty(kv.Key, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                    keyProps[kv.Value] = prop;
            }

            // Wrap the mapping in a FieldInfo record
            return new FieldInfo(keyProps);
        });
    }

    public static FieldInfo? MapperFieldInfo(this BaseCtrl? ctrl)
    {
        if (ctrl == null)
            return null;
        var fieldMappers = GetOrScanFieldMap(ctrl.GetType());
        return fieldMappers;
    }

    #endregion Cache FieldMap
}

public static partial class RaisingIndexer
{
    public static async Task<(string? sessionId, BaseCtrl? outCtrl)> JoiningData(
        this BaseCtrl? ctrl,
        Func<BaseCtrl, Task<string>>? createStringQueryMethod,
        Func<string, Type, Task<BaseCtrl?>>? getDataMethod,
        bool isLoadMapper = true,
        int recursionStopLoss = -1, // max size of recursion loop (-1 == unlimited)
        int currentRecursionLoop = 1,
        string? sessionId = null
    )
    {
        if (!IsTableLoaderAttached(ctrl))
            return (null, ctrl);

        // sessionId for joining list of data
        sessionId = !string.IsNullOrWhiteSpace(sessionId) ? sessionId : IndexerMode.CreateGuidWithStackTrace();

        // validate will pass with not null method
        if (createStringQueryMethod == null || getDataMethod == null)
            return (sessionId, ctrl);
        // validate primary keys (value)
        var primaryKeys = ctrl.PrimaryKeys()?.ToList();
        if (primaryKeys is not { Count: > 0 } ||
            primaryKeys.Any(key => key.Property.GetValue(ctrl) == null))
            return (sessionId, ctrl);

        // SqlQuery (get key)
        Type? ctrlType = ctrl?.GetType();
        // Pass both ctrl and dbType to the query creation method
        string? pkQuery = await WrapperProcessers.Processer(createStringQueryMethod!, ctrl);
        if (string.IsNullOrWhiteSpace(pkQuery))
            return (sessionId, ctrl);
        //// chim mồi được fill dữ liệu ------------
        ctrl = await WrapperProcessers.Processer(getDataMethod!, pkQuery, ctrlType);
        if (isLoadMapper)
            MapperFieldInfo(ctrl); // caching Representative Child of BaseCtrl structure
        AddToLookupData(sessionId, new DataLookup(ctrl!, null)); //GetOrScanTypeEnableObjectCache validate null

        // validate foreign keys (relationship)
        KeyInfo[]? fks = ctrl.ForeignKeys()?.ToArray();
        if (fks is not { Length: > 0 })
            return (sessionId, ctrl);
        KeyInfo[]? fkIds = ctrl.ForeignKeyIds()?.ToArray();
        if (fkIds is not { Length: > 0 })
            return (sessionId, ctrl);

        // validate attribute foreign keys and foreign key ids
        Attribute[] fkAttrs = fks.Select(x => x.Attribute).ToArray();
        Type fkAttrType = fkAttrs.First().GetType();
        if (!fkAttrType.IsGenericType || fkAttrType.GetGenericTypeDefinition() != typeof(FkAttribute<>))
            return (sessionId, ctrl);

        Attribute[] fkIdAttrs = fkIds.Select(x => x.Attribute).ToArray();
        Type fkIdAttrType = fkIdAttrs.First().GetType();
        if (fkIdAttrType.GetGenericTypeDefinition() != typeof(FkIdAttribute<>))
            return (sessionId, ctrl);

        // need add both the FkId<T> and Fk<T> when create their relationship, or break 
        Type?[] fkTypes = fkAttrs.Select(x => x.GetPropertyTypeFromAttribute(nameof(FkAttribute<>.RelatedType)))
            .ToArray();
        Type?[] fkIdsTypes = fkAttrs.Select(x => x.GetPropertyTypeFromAttribute(nameof(FkIdAttribute<>.RelatedType)))
            .ToArray();

        fkTypes = fkTypes.Where(x => fkIdsTypes.Contains(x)).ToArray();
        fkIdsTypes = fkIdsTypes.Where(x => fkTypes.Contains(x)).ToArray();

        if (fkIds.Length == 0 || fkIds.Length != fks.Length)
            return (sessionId, ctrl);

        if (recursionStopLoss >= 0 && recursionStopLoss < currentRecursionLoop)
            return (sessionId, ctrl);

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

            if (!fkType.IsChildOfBaseCtrl())
                continue;

            var ctrlFromKeyEmpty = (BaseCtrl?)Activator.CreateInstance(fkType);
            KeyInfo? navigationKeyInfo = fks.FirstOrDefault(fk =>
                fk.Attribute.GetPropertyTypeFromAttribute(nameof(FkAttribute<>.RelatedType)) == fkType);
            if (ctrlFromKeyEmpty != null)
            {
                // Get PropertyInfo 'ID' from new object
                var idPropOfNewObject = fkType.GetProperty(nameof(BaseCtrl.Id));
                if (navigationKeyInfo != null && idPropOfNewObject != null)
                {
                    object convertedIdValue = Convert.ChangeType(fkIdValue, idPropOfNewObject.PropertyType);
                    idPropOfNewObject.SetValue(ctrlFromKeyEmpty, convertedIdValue);
                }
            }

            (sessionId, BaseCtrl? fkCtrl) = await JoiningData(ctrlFromKeyEmpty, createStringQueryMethod, getDataMethod,
                isLoadMapper, // control parameters
                recursionStopLoss, ++currentRecursionLoop, sessionId); // recursions 
            navigationKeyInfo?.Property.SetValue(ctrl, fkCtrl);
        }

        return (sessionId, ctrl);
    }


    public static async Task<(string? sessionId, List<BaseCtrl>? outCtrls)> JoiningListData(
        this List<BaseCtrl>? ctrlList,
        Func<List<BaseCtrl>, Task<string>>? createBulkQueryMethod,
        Func<string, Type, Task<List<BaseCtrl>?>>? getBulkDataMethod,
        bool isLoadMapper = true,
        int recursionStopLoss = -1, // max size of recursion loop (-1 == unlimited)
        int currentRecursionLoop = 1,
        string? sessionId = null
    )
    {
        // ------ Validate Input ------
        if (ctrlList is not { Count: > 0 } || !ctrlList.All(c => c.IsTableLoaderAttached()))
            return (null, ctrlList);

        if (createBulkQueryMethod == null || getBulkDataMethod == null)
            return (sessionId, ctrlList);

        // Validate that all objects have their primary key values set.
        if (ctrlList.Any(c => c.PrimaryKeys()?.Any(pk => pk.Property.GetValue(c) == null) ?? true))
            return (sessionId, ctrlList);
        if (recursionStopLoss >= 0 && recursionStopLoss < currentRecursionLoop)
            return (sessionId, ctrlList);
        sessionId = !string.IsNullOrWhiteSpace(sessionId)
            ? sessionId
            : IndexerMode.CreateGuidWithStackTrace(); // sessionId for joining list of data 


        var groupedByType = ctrlList
            .GroupBy(t => t.GetType())
            .ToList();

        foreach (var group in groupedByType)
        {
            // ------ Fetch Bulk Data ------
            Type ctrlType = ctrlList.First().GetType();
            string? bulkQuery = await WrapperProcessers.Processer(createBulkQueryMethod!, ctrlList);
            if (string.IsNullOrWhiteSpace(bulkQuery))
                return (sessionId, ctrlList);

            List<BaseCtrl>? ctrls = await WrapperProcessers.Processer(getBulkDataMethod!, bulkQuery, ctrlType);
            if (ctrls is not { Count: > 0 })
                return (sessionId, ctrlList);

            // --- 3. Cache and Process Foreign Keys Recursively ---
            if (isLoadMapper)
                MapperFieldInfo(ctrls.First()); // Cache the field map for this type.

            // This part needs the single-object version of JoiningData to handle recursion for each item.
            // We need to create the single-object query and data retrieval methods to pass down.
            // This is a simplification; a real implementation might need more sophisticated delegate creation.
            Func<BaseCtrl, Task<string>>? createSingleQueryMethod = null; // Placeholder
            Func<string, Type, Task<BaseCtrl?>>? getSingleDataMethod = null; // Placeholder

            // If the downstream methods aren't available, we can't do recursion.
            if (createSingleQueryMethod != null && getSingleDataMethod != null)
            {
                foreach (var ctrl in ctrls)
                {
                    // Add to lookup cache
                    AddToLookupData(sessionId, new DataLookup(ctrl, null));

                    // Recursively join data for foreign keys for each object in the list.
                    // We call the *single object* overload of JoiningData here.
                    await JoiningListData(
                        ctrls,
                        createBulkQueryMethod,
                        getBulkDataMethod,
                        isLoadMapper,
                        recursionStopLoss,
                        currentRecursionLoop + 1,
                        sessionId
                    );
                }
            }

            return (sessionId, ctrls);
        }

        return (sessionId, ctrlList);
    }
}