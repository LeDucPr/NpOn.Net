using System.Reflection;
using HandlerFlow.AlgObjs.Attributes;
using HandlerFlow.AlgObjs.CtrlObjs;
using CommonMode;
using CommonObject;

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
        this BaseCtrl? decoyCtrl,
        Func<BaseCtrl, Task<string>>? createStringQueryMethod,
        Func<string, Type, Task<BaseCtrl?>>? getDataMethod,
        bool isLoadMapper = true,
        bool isUseCachingForLookupData = false,
        int recursionStopLoss = -1, // max size of recursion loop (-1 == unlimited)
        int currentRecursionLoop = 1,
        string? sessionId = null
    )
    {
        if (!IsTableLoaderAttached(decoyCtrl))
            return (null, decoyCtrl);

        // sessionId for joining list of data
        sessionId = !string.IsNullOrWhiteSpace(sessionId) ? sessionId : IndexerMode.CreateGuidWithStackTrace();

        // validate will pass with not null method
        if (createStringQueryMethod == null || getDataMethod == null)
            return (sessionId, decoyCtrl);
        // validate primary keys (value)
        var primaryKeys = decoyCtrl.PrimaryKeys()?.ToList();
        if (primaryKeys is not { Count: > 0 } ||
            primaryKeys.Any(key => key.Property.GetValue(decoyCtrl) == null))
            return (sessionId, decoyCtrl);

        // SqlQuery (get key)
        Type? ctrlType = decoyCtrl?.GetType();
        // Pass both ctrl and dbType to the query creation method
        string? pkQuery = await WrapperProcessers.Processer(createStringQueryMethod!, decoyCtrl);
        if (string.IsNullOrWhiteSpace(pkQuery))
            return (sessionId, decoyCtrl);
        //// chim mồi được fill dữ liệu ------------
        decoyCtrl = await WrapperProcessers.Processer(getDataMethod!, pkQuery, ctrlType);
        if (isLoadMapper)
            MapperFieldInfo(decoyCtrl); // caching Representative Child of BaseCtrl structure
        if (isUseCachingForLookupData)
            AddToLookupData(sessionId, new DataLookup(decoyCtrl!, null)); //GetOrScanTypeEnableObjectCache validate null

        // validate foreign keys (relationship)
        KeyInfo[]? fks = decoyCtrl.ForeignKeys()?.ToArray();
        if (fks is not { Length: > 0 })
            return (sessionId, decoyCtrl);
        KeyInfo[]? fkIds = decoyCtrl.ForeignKeyIds()?.ToArray();
        if (fkIds is not { Length: > 0 })
            return (sessionId, decoyCtrl);

        // validate attribute foreign keys and foreign key ids
        Attribute[] fkAttrs = fks.Select(x => x.Attribute).ToArray();
        Type fkAttrType = fkAttrs.First().GetType();
        if (!fkAttrType.IsGenericType || fkAttrType.GetGenericTypeDefinition() != typeof(FkAttribute<>))
            return (sessionId, decoyCtrl);

        Attribute[] fkIdAttrs = fkIds.Select(x => x.Attribute).ToArray();
        Type fkIdAttrType = fkIdAttrs.First().GetType();
        if (fkIdAttrType.GetGenericTypeDefinition() != typeof(FkIdAttribute<>))
            return (sessionId, decoyCtrl);

        // need add both the FkId<T> and Fk<T> when create their relationship, or break 
        Type?[] fkTypes = fkAttrs.Select(x => x.GetPropertyTypeFromAttribute(nameof(FkAttribute<>.RelatedType)))
            .ToArray();
        Type?[] fkIdsTypes = fkAttrs.Select(x => x.GetPropertyTypeFromAttribute(nameof(FkIdAttribute<>.RelatedType)))
            .ToArray();

        fkTypes = fkTypes.Where(x => fkIdsTypes.Contains(x)).ToArray();
        fkIdsTypes = fkIdsTypes.Where(x => fkTypes.Contains(x)).ToArray();

        if (fkIds.Length == 0 || fkIds.Length != fks.Length)
            return (sessionId, decoyCtrl);

        if (recursionStopLoss >= 0 && recursionStopLoss < currentRecursionLoop)
            return (sessionId, decoyCtrl);

        foreach (Type? fkType in fkTypes) // foreign keys
        {
            if (fkType == null)
                continue;

            if (!fkType.IsChildOfBaseCtrl())
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

            var fkIdValue = fkIdInfo.Property.GetValue(decoyCtrl);

            if (fkIdValue == null)
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

            (sessionId, BaseCtrl? fkCtrl) = await JoiningData(ctrlFromKeyEmpty,
                createStringQueryMethod, getDataMethod, // functions
                isLoadMapper, isUseCachingForLookupData, // control parameters
                recursionStopLoss, ++currentRecursionLoop, sessionId); // recursions 
            navigationKeyInfo?.Property.SetValue(decoyCtrl, fkCtrl);
        }

        return (sessionId, decoyCtrl);
    }


    public static async Task<(string? sessionId, List<BaseCtrl>? outCtrls)> JoiningListData(
        this List<BaseCtrl>? decoyCtrlList,
        Func<List<BaseCtrl>, Task<string>>? createBulkQueryMethod,
        Func<string, Type, Task<List<BaseCtrl>?>>? getBulkDataMethod,
        bool isLoadMapper = true,
        bool isUseCachingForLookupData = false,
        int recursionStopLoss = -1, // max size of recursion loop (-1 == unlimited)
        int currentRecursionLoop = 1,
        string? sessionId = null
    )
    {
        // ------ Validate Input ------
        if (decoyCtrlList is not { Count: > 0 } || !decoyCtrlList.All(c => c.IsTableLoaderAttached()))
            return (null, decoyCtrlList);

        if (createBulkQueryMethod == null || getBulkDataMethod == null)
            return (sessionId, decoyCtrlList);

        // Validate that all objects have their primary key values set.
        if (decoyCtrlList.Any(c => c.PrimaryKeys()?.Any(pk => pk.Property.GetValue(c) == null) ?? true))
            return (sessionId, decoyCtrlList);
        if (recursionStopLoss >= 0 && recursionStopLoss < currentRecursionLoop)
            return (sessionId, decoyCtrlList);
        sessionId = !string.IsNullOrWhiteSpace(sessionId)
            ? sessionId
            : IndexerMode.CreateGuidWithStackTrace(); // sessionId for joining list of data 


        var groupedByType = decoyCtrlList
            .GroupBy(t => t.GetType())
            .Select(g => new KeyValuePair<Type, List<BaseCtrl>>(g.Key, g.ToList()))
            .ToList();


        //// -> Group -> type of FK<Ctrl>(:BaseCtrl) -> Get all DataFrom ctrlList

        foreach (var group in groupedByType)
        {
            // ------ Fetch Bulk Data ------
            Type ctrlTypeOfGroup = group.Key;
            List<BaseCtrl>? groupCtrlByType = group.Value;
            string? pkQuery = await WrapperProcessers.Processer(createBulkQueryMethod!, groupCtrlByType);
            if (string.IsNullOrWhiteSpace(pkQuery))
                return (sessionId, group.Value);
            groupCtrlByType = await WrapperProcessers.Processer(getBulkDataMethod!, pkQuery, ctrlTypeOfGroup);
            if (groupCtrlByType is not { Count: > 0 })
                return (sessionId, group.Value);
            if (isLoadMapper)
                MapperFieldInfo(groupCtrlByType.First()); // caching Representative Child of BaseCtrl structure
            if (isUseCachingForLookupData) // GetOrScanTypeEnableObjectCache (validate null)
                groupCtrlByType.ForEach(ctrl => AddToLookupData(sessionId, new DataLookup(ctrl, null)));

            // validate foreign keys (relationship)
            KeyInfo[]? fks = groupCtrlByType.First().ForeignKeys()?.ToArray();
            if (fks is not { Length: > 0 })
                return (sessionId, groupCtrlByType);
            KeyInfo[]? fkIds = groupCtrlByType.First().ForeignKeyIds()?.ToArray();
            if (fkIds is not { Length: > 0 })
                return (sessionId, groupCtrlByType);

            // validate attribute foreign keys and foreign key ids
            Attribute[] fkAttrs = fks.Select(x => x.Attribute).ToArray();
            Type fkAttrType = fkAttrs.First().GetType();
            if (!fkAttrType.IsGenericType || fkAttrType.GetGenericTypeDefinition() != typeof(FkAttribute<>))
                return (sessionId, groupCtrlByType);

            Attribute[] fkIdAttrs = fkIds.Select(x => x.Attribute).ToArray();
            Type fkIdAttrType = fkIdAttrs.First().GetType();
            if (fkIdAttrType.GetGenericTypeDefinition() != typeof(FkIdAttribute<>))
                return (sessionId, groupCtrlByType);

            // need add both the FkId<T> and Fk<T> when create their relationship, or break 
            Type?[] fkTypes = fkAttrs.Select(x => x.GetPropertyTypeFromAttribute(nameof(FkAttribute<>.RelatedType)))
                .ToArray();
            Type?[] fkIdsTypes = fkAttrs
                .Select(x => x.GetPropertyTypeFromAttribute(nameof(FkIdAttribute<>.RelatedType)))
                .ToArray();

            fkTypes = fkTypes.Where(x => fkIdsTypes.Contains(x)).ToArray();
            fkIdsTypes = fkIdsTypes.Where(x => fkTypes.Contains(x)).ToArray();

            if (fkIds.Length == 0 || fkIds.Length != fks.Length)
                return (sessionId, groupCtrlByType);

            if (recursionStopLoss >= 0 && recursionStopLoss < currentRecursionLoop)
                return (sessionId, groupCtrlByType);

            foreach (Type? fkType in fkTypes) // foreign keys
            {
                if (fkType == null)
                    continue;

                if (!fkType.IsChildOfBaseCtrl())
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

                List<BaseCtrl>? ctrlFromKeyEmpties = [];
                HashSet<string?> idList = [];
                KeyInfo? navigationKeyInfo = fks.FirstOrDefault(fk =>
                    fk.Attribute.GetPropertyTypeFromAttribute(nameof(FkAttribute<>.RelatedType)) == fkType);

                // Add Value to ForeignKey Field
                foreach (var ctrl in groupCtrlByType)
                {
                    var ctrlFromKeyEmpty = (BaseCtrl?)Activator.CreateInstance(fkType);
                    var fkIdValue = fkIdInfo.Property.GetValue(ctrl);
                    if (fkIdValue == null)
                        continue;
                    if (ctrlFromKeyEmpty != null)
                    {
                        // Get PropertyInfo 'ID' from new object
                        var idPropOfNewObject = fkType.GetProperty(nameof(BaseCtrl.Id));
                        if (navigationKeyInfo != null && idPropOfNewObject != null)
                        {
                            object convertedIdValue = Convert.ChangeType(fkIdValue, idPropOfNewObject.PropertyType);
                            idPropOfNewObject.SetValue(ctrlFromKeyEmpty, convertedIdValue);
                            idList.Add(convertedIdValue.ToString());
                        }

                        // When you add above, you also add below.
                        if (idList.Count != ctrlFromKeyEmpties.Count)
                            ctrlFromKeyEmpties.Add(ctrlFromKeyEmpty);
                    }
                }

                (sessionId, ctrlFromKeyEmpties) = await JoiningListData(ctrlFromKeyEmpties, // objects
                    createBulkQueryMethod, getBulkDataMethod, // functions 
                    isLoadMapper, isUseCachingForLookupData, // control parameters
                    recursionStopLoss, ++currentRecursionLoop, sessionId); // recursions 

                foreach (var ctrl in groupCtrlByType)
                {
                    var fkIdValue = fkIdInfo.Property.GetValue(ctrl);
                    if (fkIdValue == null)
                        continue;
                    var ctrlByFkId = ctrlFromKeyEmpties?.FirstOrDefault(x => x.Id.ToString() == fkIdValue.ToString());
                    if (ctrlByFkId == null)
                        continue;
                    var idPropOfNewObject = fkType.GetProperty(nameof(BaseCtrl.Id));
                    navigationKeyInfo?.Property.SetValue(ctrl, ctrlByFkId); // may be null ??
                }
            }

            return (sessionId, groupCtrlByType);
        }

        return (sessionId, decoyCtrlList);
    }
}