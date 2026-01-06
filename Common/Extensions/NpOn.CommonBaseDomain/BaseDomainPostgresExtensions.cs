using System.Reflection;
using System.Text;
using Common.Extensions.NpOn.CommonBaseDomain.Attributes;
using Common.Extensions.NpOn.HandleFlow.Attributes;
using Common.Infrastructures.NpOn.PostgresExtCm.Results;
using Npgsql;
using NpgsqlTypes;

namespace Common.Extensions.NpOn.CommonBaseDomain;

public static partial class BaseDomainExtensions
{
    public static (string CommandText, List<NpgsqlParameter> Parameters) ToPostgresParamsInsert
        (this List<BaseDomain> domains, bool isUseDefaultWhenNull = false)
    {
        if (domains == null || domains.Count == 0)
            throw new Exception("Empty domain list");

        if (!domains.TryGetSingleTableAttribute(out var tableLoader) || tableLoader == null)
            throw new Exception("Invalid table attribute");

        var type = domains[0].GetType();
        if (domains.Any(d => d.GetType() != type))
            throw new Exception("All domains must be of the same type");

        // 1) Pre-scan: collect [Field] mappings (column -> member), independent of property names
        var mappedMembers = GetFieldMappedMembers(type); // (columnName, member, memberType)
        if (mappedMembers.Count == 0)
            throw new Exception($"Type {type.Name} has no [Field] mapped members");

        var parameters = new List<NpgsqlParameter>();
        var sql = new StringBuilder();

        // 2) Build one INSERT per row to truly skip nulls without column mismatch
        for (int i = 0; i < domains.Count; i++)
        {
            var cols = new List<string>();
            var paramNames = new List<string>();

            foreach (var (columnName, member, memberType) in mappedMembers)
            {
                var raw = GetMemberValue(member, domains[i]);
                if (!isUseDefaultWhenNull && (raw == null || IsDefaultValue(raw, memberType)))
                {
                    continue;
                }

                var (paramValue, npgType) = NormalizeForNpgsql(raw, memberType);
                string param = $"@p_{i}_{cols.Count}";
                cols.Add(columnName);
                paramNames.Add(param);

                var p = new NpgsqlParameter(param, paramValue ?? DBNull.Value);
                if (npgType.HasValue) p.NpgsqlDbType = npgType.Value;
                parameters.Add(p);
            }

            if (cols.Count == 0) continue; // nothing to insert for this row

            sql.Append(
                $"INSERT INTO {tableLoader.TableName} ({string.Join(",", cols)}) VALUES ({string.Join(",", paramNames)});");
        }

        return (sql.ToString(), parameters);
    }


    public static (string CommandText, List<NpgsqlParameter> Parameters) ToPostgresParamsUpdate(
        this List<BaseDomain> domains, bool isUseDefaultWhenNull = false)
    {
        if (domains == null || domains.Count == 0)
            throw new Exception("Empty domain list");

        var type = domains[0].GetType();
        var mappedMembers = GetFieldMappedMembers(type);
        var pkMembers = mappedMembers.Where(m => m.Member.GetCustomAttributes(typeof(PkAttribute), true).Any())
            .ToList();
        if (!pkMembers.Any())
            throw new Exception($"Type {type.Name} has no primary key");

        var parameters = new List<NpgsqlParameter>();
        var sql = new StringBuilder();
        var tableName = type.GetCustomAttribute<TableLoaderAttribute>()?.TableName;

        // Foreach domain → create UPDATE command
        for (int i = 0; i < domains.Count; i++)
        {
            var setClauses = new List<string>();

            foreach (var (colName, member, memberType) in mappedMembers)
            {
                // skip PK
                if (pkMembers.Any(pk => pk.ColumnName == colName))
                    continue;

                var raw = GetMemberValue(member, domains[i]);
                if (!isUseDefaultWhenNull && (raw == null || IsDefaultValue(raw, memberType)))
                {
                    continue;
                }

                var (val, npgType) = NormalizeForNpgsql(raw, memberType);
                string param = $"@v_{i}_{colName}";
                var p = new NpgsqlParameter(param, val ?? DBNull.Value);
                if (npgType.HasValue) p.NpgsqlDbType = npgType.Value;
                parameters.Add(p);

                setClauses.Add($"{colName} = {param}");
            }

            if (setClauses.Count == 0) continue;

            var pkConditions = new List<string>();
            for (int j = 0; j < pkMembers.Count; j++)
            {
                var pkMember = pkMembers[j];
                string pkParam = $"@pk_{i}_{j}";
                var pkVal = GetMemberValue(pkMember.Member, domains[i]);
                if (pkVal == null)
                    throw new Exception(
                        $"Primary key value for {pkMember.ColumnName} cannot be null in domain at index {i}");

                var (pkValue, pkType) = NormalizeForNpgsql(pkVal, pkMember.MemberType);
                var pkParamObj = new NpgsqlParameter(pkParam, pkValue);
                if (pkType.HasValue) pkParamObj.NpgsqlDbType = pkType.Value;
                parameters.Add(pkParamObj);
                pkConditions.Add($"{pkMember.ColumnName} = {pkParam}");
            }

            sql.Append(
                $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE {string.Join(" AND ", pkConditions)};");
        }

        return (sql.ToString(), parameters);
    }


    public static (string CommandText, List<NpgsqlParameter> Parameters) ToPostgresParamsMerge(
        this List<BaseDomain> domains, bool isUseDefaultWhenNull = false)
    {
        if (domains == null || domains.Count == 0)
            throw new Exception("Empty domain list");

        var type = domains[0].GetType();
        var mappedMembers = GetFieldMappedMembers(type);
        var pkMembers = mappedMembers.Where(m => m.Member.GetCustomAttributes(typeof(PkAttribute), true).Any())
            .ToList();
        if (!pkMembers.Any())
            throw new Exception($"Type {type.Name} has no primary key");

        var tableName = type.GetCustomAttribute<TableLoaderAttribute>()?.TableName;
        if (string.IsNullOrEmpty(tableName))
            throw new Exception($"Type {type.Name} is missing TableLoaderAttribute");

        var parameters = new List<NpgsqlParameter>();
        var sql = new StringBuilder();

        // Build one INSERT per row to handle nulls correctly
        for (int i = 0; i < domains.Count; i++)
        {
            var domain = domains[i];
            var cols = new List<string>();
            var paramNames = new List<string>();

            foreach (var (colName, member, memberType) in mappedMembers)
            {
                var raw = GetMemberValue(member, domain);
                // Skip properties with null or default value type (e.g., Guid.Empty)
                // This allows the database to use its default value (e.g., gen_random_uuid())
                if (!isUseDefaultWhenNull && (raw == null || IsDefaultValue(raw, memberType)))
                {
                    continue;
                }

                var (val, npgType) = NormalizeForNpgsql(raw, memberType);
                string param = $"@p_{i}_{colName}";

                cols.Add(colName);
                paramNames.Add(param);

                var p = new NpgsqlParameter(param, val ?? DBNull.Value);
                if (npgType.HasValue) p.NpgsqlDbType = npgType.Value;
                parameters.Add(p);
            }

            if (cols.Count == 0) continue; // Nothing to insert for this domain

            sql.Append($"INSERT INTO {tableName} ({string.Join(",", cols)}) VALUES ({string.Join(",", paramNames)})");
            sql.Append($" ON CONFLICT ({string.Join(",", pkMembers.Select(m => m.ColumnName))}) DO UPDATE SET ");

            var updateSetClauses = mappedMembers.Where(m =>
                    pkMembers.All(pk => pk.ColumnName != m.ColumnName) && cols.Contains(m.ColumnName))
                .Select(m => $"{m.ColumnName} = EXCLUDED.{m.ColumnName}");
            sql.Append(string.Join(", ", updateSetClauses));
            sql.Append(';');
        }

        return (sql.ToString(), parameters);
    }


    public static (string CommandText, List<NpgsqlParameter> Parameters) ToPostgresParamsDelete(
        this List<BaseDomain> domains, bool isUseDefaultWhenNull = false)
    {
        if (domains == null || domains.Count == 0)
            throw new Exception("Empty domain list");

        var type = domains[0].GetType();
        var mappedMembers = GetFieldMappedMembers(type);
        var pkMembers = mappedMembers
            .Where(m => m.Member.GetCustomAttributes(typeof(PkAttribute), true).Any())
            .ToList();

        if (!pkMembers.Any())
            throw new Exception($"Type {type.Name} has no primary key");

        var parameters = new List<NpgsqlParameter>();
        var sql = new StringBuilder();

        var tableName = type.GetCustomAttribute<TableLoaderAttribute>()?.TableName;
        sql.Append($"DELETE FROM {tableName} WHERE ");

        var whereClauses = new List<string>();
        for (int i = 0; i < domains.Count; i++)
        {
            var domain = domains[i];
            var pkConditions = new List<string>();

            for (int j = 0; j < pkMembers.Count; j++)
            {
                var pkMember = pkMembers[j];
                string pkParam = $"@pk_{i}_{j}";
                var raw = GetMemberValue(pkMember.Member, domain);

                if (raw == null)
                {
                    if (isUseDefaultWhenNull)
                    {
                        object defaultVal = pkMember.MemberType.IsValueType
                            ? Activator.CreateInstance(pkMember.MemberType)!
                            : string.Empty;

                        raw = defaultVal;
                    }
                    else
                    {
                        continue;
                    }
                }

                var (val, npgType) = NormalizeForNpgsql(raw, pkMember.MemberType);
                var p = new NpgsqlParameter(pkParam, val);
                if (npgType.HasValue) p.NpgsqlDbType = npgType.Value;
                parameters.Add(p);
                pkConditions.Add($"{pkMember.ColumnName} = {pkParam}");
            }

            if (pkConditions.Any())
            {
                whereClauses.Add($"({string.Join(" AND ", pkConditions)})");
            }
        }

        if (!whereClauses.Any())
            throw new Exception("No valid primary key conditions generated");

        sql.Append(string.Join(" OR ", whereClauses));

        return (sql.ToString(), parameters);
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

    #region private

    private static List<(string ColumnName, MemberInfo Member, Type MemberType)>
        GetFieldMappedMembers(Type type)
    {
        var list = new List<(string, MemberInfo, Type)>();

        foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var fa = p.GetCustomAttributes(true).OfType<FieldAttribute>().FirstOrDefault();
            if (fa != null && !string.IsNullOrWhiteSpace(fa.FieldName))
                list.Add((fa.FieldName, p, p.PropertyType));
        }

        foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var fa = f.GetCustomAttributes(true).OfType<FieldAttribute>().FirstOrDefault();
            if (fa != null && !string.IsNullOrWhiteSpace(fa.FieldName))
                list.Add((fa.FieldName, f, f.FieldType));
        }

        return list;
    }

    private static object? GetMemberValue(MemberInfo member, object instance)
    {
        return member switch
        {
            PropertyInfo pi => pi.GetValue(instance),
            FieldInfo fi => fi.GetValue(instance),
            _ => null
        };
    }

    // Convert enums and common types to safe Npgsql representations by using the centralized utility
    private static (object? Value, NpgsqlDbType? DbType) NormalizeForNpgsql(object raw, Type memberType)
    {
        return PostgresUtils.NormalizeValueForNpgsql(raw, memberType);
    }

    /// <summary>
    /// Checks if a value is the default for its type (e.g., 0 for int, Guid.Empty for Guid).
    /// This is used to avoid inserting default values for columns that have database-side defaults (like auto-generated UUIDs).
    /// </summary>
    private static bool IsDefaultValue(object value, Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (!underlyingType.IsValueType)
            return false; // Reference types are already handled by the null check.

        if (underlyingType == typeof(Guid) && value is Guid g) return g == Guid.Empty;
        if (underlyingType == typeof(DateTime) && value is DateTime d) return d == DateTime.MinValue;

        return false;
    }

    #endregion private
}