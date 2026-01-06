using System.Reflection;
using Common.Extensions.NpOn.CommonInternalCache;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.HandleFlow.Attributes;
using FieldInfo = Common.Extensions.NpOn.HandleFlow.Raising.FieldInfo;

namespace Common.Extensions.NpOn.HandleFlow.Raising;

public static class RaisingIndexer
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
        var fieldMap = emptyCtrl?.FieldMap ?? ctrlType.CreateDefaultFieldMapperWithEmptyBaseCtrlObject()?.FieldMap;
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
