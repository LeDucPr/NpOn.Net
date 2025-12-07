using System.Text;
using CommonObject;
using Enums;
using Npgsql;
using ObjectHandlerFlow.AlgObjs.Attributes;

namespace ProjectBaseDomain;

public static class BaseDomainExtensions
{
    public static NpgsqlCommand? ToPostgresParamsInsert(this List<BaseDomain>? domains, EDbAction action)
    {
        if (domains == null || domains.Count == 0)
            return null;

        if (!domains.TryGetSingleTableAttribute(out var tableLoader) || tableLoader == null)
            return null;
        
        var type = domains.First().GetType();
        var meta = DomainMetadataCache.GetMetadata(type);

        // domains as the same type
        if (domains.Any(d => d.GetType() != type))
            throw new Exception("All domains must be of the same type");

        var sql = new StringBuilder();
        sql.Append($"INSERT INTO {meta.TableName} ({meta.ColumnNames.AsArrayJoin()}) VALUES ");

        var cmd = new NpgsqlCommand();

        for (int i = 0; i < domains.Count; i++)
        {
            var paramNames = new List<string>();

            for (int c = 0; c < meta.Getters.Count; c++)
            {
                string param = $"@p_{i}_{c}";
                paramNames.Add(param);
                object value = meta.Getters[c](domains[i]) ?? DBNull.Value;
                cmd.Parameters.AddWithValue(param, value);
            }

            sql.Append($"({paramNames.AsArrayJoin()}),");
        }

        sql.Length--; // remove last comma
        cmd.CommandText = sql.ToString();
        return cmd;
    }


    private static bool TryGetSingleTableAttribute(
        this IEnumerable<BaseDomain> domains,
        out TableLoaderAttribute? tableAttr)
    {
        tableAttr = null;
        var validDomains = domains
            .Where(x => x is BaseDomain)
            .ToList();
        if (!validDomains.Any())
            return false;

        var attrs = validDomains
            .Select(x => x.GetType()
                .GetCustomAttributes(typeof(TableLoaderAttribute), true)
                .FirstOrDefault() as TableLoaderAttribute)
            .ToList();

        if (attrs.Any(a => a == null))
            return false;

        var distinctTables = attrs
            .Select(a => a!.TableName)
            .Distinct()
            .ToList();

        if (distinctTables.Count != 1)
            return false;

        tableAttr = attrs.First();
        return true;
    }
}