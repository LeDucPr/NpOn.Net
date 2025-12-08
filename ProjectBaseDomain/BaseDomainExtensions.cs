using System.Text;
using CommonObject;
using Npgsql;
using ObjectHandlerFlow.AlgObjs.Attributes;

namespace ProjectBaseDomain;

public static class BaseDomainExtensions
{
    public static NpgsqlCommand ToPostgresParamsInsert(this List<BaseDomain> domains)
    {
        if (domains == null || domains.Count == 0)
            throw new Exception("Empty domain list");

        if (!domains.TryGetSingleTableAttribute(out var tableLoader) || tableLoader == null)
            throw new Exception("Invalid table attribute");

        var type = domains.First().GetType();
        var meta = DomainMetadataCache.GetMetadata(type);

        if (domains.Any(d => d.GetType() != type))
            throw new Exception("All domains must be of the same type");

        var cmd = new NpgsqlCommand();
        var sql = new StringBuilder();

        sql.Append($"INSERT INTO {meta.TableName} ({meta.ColumnNames.AsArrayJoin()}) VALUES ");

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

        sql.Length--; // Delete last comma 
        cmd.CommandText = sql.ToString();
        return cmd;
    }

    public static NpgsqlCommand ToPostgresParamsUpdate(this List<BaseDomain> domains)
    {
        if (domains == null || domains.Count == 0)
            throw new Exception("Empty domain list");

        var type = domains.First().GetType();
        var meta = DomainMetadataCache.GetMetadata(type);

        if (!meta.PrimaryKeys.Any())
            throw new Exception($"Type {type.Name} has no primary key");

        var cmd = new NpgsqlCommand();
        var sql = new StringBuilder();

        sql.Append($"UPDATE {meta.TableName} SET ");

        // Build SET col = CASE WHEN pk THEN value END
        for (int col = 0; col < meta.ColumnNames.Count; col++)
        {
            string colName = meta.ColumnNames[col];
            // do not update PK
            if (meta.PrimaryKeys.Contains(colName))
                continue;
            sql.Append($"{colName} = CASE ");
            for (int i = 0; i < domains.Count; i++)
            {
                // PK param
                string pkParam = $"@pk_{i}";
                object pkValue = meta.PrimaryKeyGetters[0](domains[i]) ?? DBNull.Value;
                cmd.Parameters.AddWithValue(pkParam, pkValue);

                // Value param
                string valParam = $"@v_{i}_{col}";
                object colValue = meta.Getters[col](domains[i]) ?? DBNull.Value;
                cmd.Parameters.AddWithValue(valParam, colValue);

                sql.Append($"WHEN {meta.PrimaryKeys[0]} = {pkParam} THEN {valParam} ");
            }

            sql.Append("END, ");
        }

        sql.Length -= 2;
        cmd.CommandText = sql.ToString();
        return cmd;
    }

    public static NpgsqlCommand ToPostgresParamsMerge(this List<BaseDomain> domains)
    {
        if (domains == null || domains.Count == 0)
            throw new Exception("Empty domain list");

        var type = domains.First().GetType();
        var meta = DomainMetadataCache.GetMetadata(type);

        if (!meta.PrimaryKeys.Any())
            throw new Exception($"Type {type.Name} has no primary key");

        var cmd = new NpgsqlCommand();
        var sql = new StringBuilder();

        sql.Append($"INSERT INTO {meta.TableName} ({meta.ColumnNames.AsArrayJoin()}) VALUES ");

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

        sql.Length--;

        // ON CONFLICT (pk)
        sql.Append($" ON CONFLICT ({meta.PrimaryKeys.AsArrayJoin()}) DO UPDATE SET ");
        for (int col = 0; col < meta.ColumnNames.Count; col++)
        {
            string colName = meta.ColumnNames[col];
            if (meta.PrimaryKeys.Contains(colName))
                continue;
            sql.Append($"{colName} = EXCLUDED.{colName}, ");
        }

        sql.Length -= 2;
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