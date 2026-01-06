using System.Runtime.CompilerServices;
using Common.Extensions.NpOn.CommonBaseDomain.Attributes;
using Common.Extensions.NpOn.HandleFlow.Attributes;

namespace Common.Extensions.NpOn.CommonBaseDomain;

public class DomainMetadata
{
    public string TableName { get; set; } = "";
    public List<string> ColumnNames { get; set; } = new();
    public List<Func<BaseDomain, object?>> Getters { get; set; } = new();
    public List<string> PrimaryKeys { get; set; } = new(); //  PK
    public List<Func<BaseDomain, object?>> PrimaryKeyGetters { get; set; } = new(); // Getter PK
}
public static class DomainMetadataCache
{
    private static readonly ConditionalWeakTable<Type, DomainMetadata> _cache = new();

    public static DomainMetadata GetMetadata(Type type)
    {
        if (_cache.TryGetValue(type, out var meta))
            return meta;

        meta = BuildMetadata(type);
        _cache.Add(type, meta);
        return meta;
    }

    private static DomainMetadata BuildMetadata(Type type)
    {
        var meta = new DomainMetadata();

        // Table attribute
        var tableAttr = type.GetCustomAttributes(typeof(TableLoaderAttribute), true)
            .FirstOrDefault() as TableLoaderAttribute
            ?? throw new Exception($"Missing TableLoaderAttribute on {type.Name}");

        meta.TableName = tableAttr.TableName;

        var props = type.GetProperties();
        var fields = type.GetFields();

        // ✅ FieldAttribute mapping
        foreach (var p in props)
        {
            if (p.GetCustomAttributes(typeof(FieldAttribute), true).FirstOrDefault() is FieldAttribute f)
            {
                meta.ColumnNames.Add(f.FieldName);
                meta.Getters.Add((BaseDomain d) => p.GetValue(d));

                // ✅ PKAttribute
                if (p.GetCustomAttributes(typeof(PkAttribute), true).Any())
                {
                    meta.PrimaryKeys.Add(f.FieldName);
                    meta.PrimaryKeyGetters.Add((BaseDomain d) => p.GetValue(d));
                }
            }
        }

        foreach (var f in fields)
        {
            if (f.GetCustomAttributes(typeof(FieldAttribute), true).FirstOrDefault() is FieldAttribute fa)
            {
                meta.ColumnNames.Add(fa.FieldName);
                meta.Getters.Add((BaseDomain d) => f.GetValue(d));

                if (f.GetCustomAttributes(typeof(PkAttribute), true).Any())
                {
                    meta.PrimaryKeys.Add(fa.FieldName);
                    meta.PrimaryKeyGetters.Add((BaseDomain d) => f.GetValue(d));
                }
            }
        }
        
        // if (!meta.PrimaryKeys.Any())
        //     throw new Exception($"Type {type.Name} does not define any primary key (PkAttribute)");

        return meta;
    }
}
