using System.Text.RegularExpressions;
using NpgsqlTypes;
using Tools.PgGenFldCode.Parser;

namespace Tools.PgGenFldCode.Object;

public sealed class PgParamTypeResolver
{
    private readonly PgMetadataProvider _meta;

    public PgParamTypeResolver(PgMetadataProvider meta)
    {
        _meta = meta;
    }
    public NpgsqlDbType Resolve(
        string paramName,
        string sql,
        Dictionary<string, TableRef> aliasMap)
    {
        // 1. CAST
        var castMatch = Regex.Match(sql,
            $@"@{paramName}\s*::\s*(?<t>[\w\[\]]+)",
            RegexOptions.IgnoreCase);

        if (castMatch.Success)
        {
            var pgType = castMatch.Groups["t"].Value;
            return PgTypeMapper.Map(
                pgType.EndsWith("[]") ? "array" : pgType,
                pgType.EndsWith("[]") ? "_" + pgType[..^2] : null);
        }

        // 2. Column comparison
        var colMatch = Regex.Match(sql,
            $@"(?:(?<a>\w+)\.)?(?<c>\w+)\s*=\s*@{paramName}",
            RegexOptions.IgnoreCase);

        if (colMatch.Success)
        {
            var alias = colMatch.Groups["a"].Value;
            var col = colMatch.Groups["c"].Value;

            if (!string.IsNullOrEmpty(alias) && aliasMap.TryGetValue(alias, out var tbl))
            {
                var meta = _meta.GetColumns(tbl.Schema, tbl.Name)
                    .FirstOrDefault(x => x.ColumnName.Equals(col, StringComparison.OrdinalIgnoreCase));
                if (meta != null) return PgTypeMapper.Map(meta.DataType, meta.UdtName);
            }
            else if (string.IsNullOrEmpty(alias))
            {
                foreach (var t in aliasMap.Values)
                {
                    var meta = _meta.GetColumns(t.Schema, t.Name)
                        .FirstOrDefault(x => x.ColumnName.Equals(col, StringComparison.OrdinalIgnoreCase));
                    if (meta != null) return PgTypeMapper.Map(meta.DataType, meta.UdtName);
                }
            }
        }

        // 3. Context inference (fallback)
        if (Regex.IsMatch(sql, $@"\b(limit|offset)\s+@{paramName}\b",
                RegexOptions.IgnoreCase))
            return NpgsqlDbType.Integer;

        if (Regex.IsMatch(sql,
                $@"nullif\s*\(\s*@{paramName}\s*,\s*''\s*\)",
                RegexOptions.IgnoreCase))
            return NpgsqlDbType.Text;

        if (Regex.IsMatch(sql,
                $@"(ilike|like).+@{paramName}|@{paramName}\s*\|\|",
                RegexOptions.IgnoreCase))
            return NpgsqlDbType.Text;

        if (Regex.IsMatch(sql,
                $@"@{paramName}\s*[\*\+\-/]",
                RegexOptions.IgnoreCase))
            return NpgsqlDbType.Numeric;

        // 4. Fail fast
        return NpgsqlDbType.Unknown;
    }

}
