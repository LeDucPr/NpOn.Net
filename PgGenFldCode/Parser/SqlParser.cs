using System.Text.RegularExpressions;
using Npgsql;

namespace PgGenFldCode.Parser;

// ===== AST models =====
public class QueryNode
{
    public List<SelectItem> SelectItems { get; set; } = new();
    public List<FromSource> FromSources { get; set; } = new();
    public Dictionary<string, QueryNode> Ctes { get; set; } = new(); // name -> query
}

public abstract class FromSource
{
    public string Alias { get; set; }
}

public class TableRef : FromSource
{
    public string Schema { get; set; } // nullable: default 'public'
    public string Name { get; set; }
}

public class SubqueryRef : FromSource
{
    public QueryNode Subquery { get; set; }
}

public abstract class SelectItem { }

public class ColumnRef : SelectItem
{
    public string TableAlias { get; set; } // nullable for bare column
    public string ColumnName { get; set; }
    public string Alias { get; set; } // output alias
}

public class WildcardRef : SelectItem
{
    public string TableAlias { get; set; } // null => global *
    public string Alias { get; set; } // không dùng cho wildcard; để đồng nhất api
}

public class ExpressionRef : SelectItem
{
    public string ExpressionSql { get; set; }
    public string Alias { get; set; }
}

// ===== Parser =====
public class SqlParser
{
    private readonly string _sql;

    public SqlParser(string sql)
    {
        _sql = Normalize(sql);
    }

    public QueryNode Parse()
    {
        var idx = 0;
        var ctes = ParseCtes(ref idx);
        var query = ParseSelect(ref idx);
        query.Ctes = ctes;
        return query;
    }

    private static string Normalize(string s)
    {
        return Regex.Replace(s, @"\s+", " ").Trim();
    }

    private Dictionary<string, QueryNode> ParseCtes(ref int idx)
    {
        var ctes = new Dictionary<string, QueryNode>(StringComparer.OrdinalIgnoreCase);
        if (!StartsWithKeyword(_sql, idx, "WITH")) return ctes;

        idx = MovePastKeyword(_sql, idx, "WITH");

        while (true)
        {
            var name = ReadIdentifier(_sql, ref idx);
            if (string.IsNullOrEmpty(name))
                throw new Exception("CTE name expected.");

            SkipOptionalParenthesizedIdentifierList(_sql, ref idx);

            idx = SkipWhitespace(_sql, idx);
            if (!StartsWithKeyword(_sql, idx, "AS"))
                throw new Exception("AS expected in CTE.");
            idx = MovePastKeyword(_sql, idx, "AS");

            idx = SkipWhitespace(_sql, idx);
            if (idx >= _sql.Length || _sql[idx] != '(')
                throw new Exception("CTE subquery expected.");

            var subquerySql = ReadParenthesizedBlock(_sql, ref idx).Trim();
            if (subquerySql.StartsWith("(") && subquerySql.EndsWith(")"))
                subquerySql = subquerySql.Substring(1, subquerySql.Length - 2).Trim();

            var subParser = new SqlParser(subquerySql);
            var subNode = subParser.ParseSelectOnly();
            ctes[name] = subNode;

            idx = SkipWhitespace(_sql, idx);
            if (idx >= _sql.Length || _sql[idx] != ',') break;
            idx++;
        }
        return ctes;
    }

    private QueryNode ParseSelectOnly()
    {
        var idx = 0;
        return ParseSelect(ref idx);
    }

    private QueryNode ParseSelect(ref int idx)
    {
        idx = SkipWhitespace(_sql, idx);
        if (idx < _sql.Length && _sql[idx] == '(')
        {
            idx++;
            idx = SkipWhitespace(_sql, idx);
        }

        if (idx >= _sql.Length || !StartsWithKeyword(_sql, idx, "SELECT"))
            throw new Exception($"SELECT expected at position {idx}. Context: {SafeContext(_sql, idx)}");

        idx = MovePastKeyword(_sql, idx, "SELECT");

        var selectListSql = ReadUntilKeywordsOrParenBalanced(_sql, ref idx, new[] { "FROM" });
        var selectItems = ParseSelectList(selectListSql);

        var query = new QueryNode { SelectItems = selectItems };

        idx = SkipWhitespace(_sql, idx);
        if (idx < _sql.Length && StartsWithKeyword(_sql, idx, "FROM"))
        {
            idx = MovePastKeyword(_sql, idx, "FROM");
            var fromSql = ReadUntilKeywordsOrParenBalanced(_sql, ref idx,
                new[] { "WHERE", "GROUP", "HAVING", "ORDER", "LIMIT", "OFFSET", "UNION", "INTERSECT", "EXCEPT" });
            var sources = ParseFromSources(fromSql);
            query.FromSources = sources;
        }
        return query;
    }

    private static string SafeContext(string sql, int idx)
    {
        if (idx < 0) idx = 0;
        if (idx >= sql.Length) return "";
        var len = Math.Min(40, sql.Length - idx);
        return sql.Substring(idx, len);
    }

    private List<SelectItem> ParseSelectList(string selectSql)
    {
        var items = SplitTopLevel(selectSql, ',');
        var list = new List<SelectItem>();
        foreach (var raw in items)
        {
            var s = raw.Trim();
            if (s == "*")
            {
                list.Add(new WildcardRef { TableAlias = null });
                continue;
            }

            var starIdx = s.IndexOf(".*", StringComparison.Ordinal);
            if (starIdx > 0 && starIdx == s.Length - 2)
            {
                var alias = s.Substring(0, starIdx).Trim();
                list.Add(new WildcardRef { TableAlias = alias });
                continue;
            }

            var asMatch = Regex.Match(s, @"^(.*?)(?:\s+AS\s+|\s+)([A-Za-z_][A-Za-z0-9_]*)$", RegexOptions.IgnoreCase);
            if (asMatch.Success)
            {
                var left = asMatch.Groups[1].Value.Trim();
                var alias = asMatch.Groups[2].Value.Trim();
                var colMatch = Regex.Match(left, @"^(?:(?<t>[A-Za-z_][A-Za-z0-9_]*)\.)?(?<c>[A-Za-z_][A-Za-z0-9_]*)$",
                    RegexOptions.IgnoreCase);
                if (colMatch.Success)
                {
                    list.Add(new ColumnRef
                    {
                        TableAlias = colMatch.Groups["t"].Success ? colMatch.Groups["t"].Value : null,
                        ColumnName = colMatch.Groups["c"].Value,
                        Alias = alias
                    });
                }
                else
                {
                    list.Add(new ExpressionRef { ExpressionSql = left, Alias = alias });
                }
                continue;
            }

            var colMatch2 = Regex.Match(s, @"^(?:(?<t>[A-Za-z_][A-Za-z0-9_]*)\.)?(?<c>[A-Za-z_][A-Za-z0-9_]*)$",
                RegexOptions.IgnoreCase);
            if (colMatch2.Success)
            {
                list.Add(new ColumnRef
                {
                    TableAlias = colMatch2.Groups["t"].Success ? colMatch2.Groups["t"].Value : null,
                    ColumnName = colMatch2.Groups["c"].Value,
                    Alias = null
                });
            }
            else
            {
                list.Add(new ExpressionRef { ExpressionSql = s, Alias = null });
            }
        }
        return list;
    }

    private List<FromSource> ParseFromSources(string fromSql)
    {
        var sources = new List<FromSource>();

        // Tách theo từ khóa JOIN ở mức top-level
        var parts = Regex.Split(fromSql, @"\bJOIN\b", RegexOptions.IgnoreCase);

        foreach (var part in parts)
        {
            var s = part.Trim();
            if (string.IsNullOrWhiteSpace(s)) continue;

            // Nếu bắt đầu bằng ON thì bỏ qua (điều kiện join)
            if (s.StartsWith("ON", StringComparison.OrdinalIgnoreCase)) continue;

            // Nếu là subquery
            if (s.StartsWith("("))
            {
                var sub = ExtractParenthesized(s);
                var inner = sub.Trim();
                if (inner.StartsWith("(") && inner.EndsWith(")"))
                    inner = inner.Substring(1, inner.Length - 2).Trim();

                var alias = s.Substring(sub.Length).Trim();
                if (string.IsNullOrEmpty(alias))
                    throw new Exception("Subquery must have alias.");

                var subParser = new SqlParser(inner);
                sources.Add(new SubqueryRef { Subquery = subParser.Parse(), Alias = alias });
            }
            else
            {
                var primary = ReadPrimaryTableToken(s);
                sources.Add(primary);
            }
        }

        return sources;
    }

    private FromSource ReadPrimaryTableToken(string s)
    {
        var cutIdx = s.IndexOf(" ON ", StringComparison.OrdinalIgnoreCase);
        var usingIdx = s.IndexOf(" USING ", StringComparison.OrdinalIgnoreCase);
        int end = s.Length;
        if (cutIdx >= 0) end = Math.Min(end, cutIdx);
        if (usingIdx >= 0) end = Math.Min(end, usingIdx);
        var head = s.Substring(0, end).Trim();

        var tokens = TokenizeIdentifiers(head);
        if (tokens.Count == 0)
            throw new Exception($"Invalid FROM source: {s}");

        string schema = null, table = null, alias = null;

        var first = tokens[0];
        if (first.Contains('.'))
        {
            var parts = first.Split('.', 2);
            schema = parts[0];
            table = parts[1];
        }
        else
        {
            table = first;
        }

        if (tokens.Count >= 2 && !IsJoinKeyword(tokens[1]))
        {
            alias = tokens[1];
        }

        return new TableRef { Schema = schema, Name = table, Alias = alias ?? table };  
    }

    private static bool IsJoinKeyword(string s)
    {
        s = s.ToUpperInvariant();
        return s is "JOIN" or "LEFT" or "RIGHT" or "FULL" or "INNER" or "OUTER" or "CROSS" or "NATURAL" or "ON" or "USING";
    }

    // --- Utility parsing functions ---
    private static bool StartsWithKeyword(string sql, int idx, string kw)
    {
        var span = sql.AsSpan(idx);
        return span.StartsWith(kw.AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    private static int MovePastKeyword(string sql, int idx, string kw)
    {
        idx += kw.Length;
        return SkipWhitespace(sql, idx);
    }

    private static int SkipWhitespace(string sql, int idx)
    {
        while (idx < sql.Length && char.IsWhiteSpace(sql[idx])) idx++;
        return idx;
    }

    private static string ReadIdentifier(string sql, ref int idx)
    {
        idx = SkipWhitespace(sql, idx);
        var start = idx;
        while (idx < sql.Length && (char.IsLetterOrDigit(sql[idx]) || sql[idx] == '_' || sql[idx] == '.')) idx++;
        return sql.Substring(start, idx - start);
    }

    private static void SkipOptionalParenthesizedIdentifierList(string sql, ref int idx)
    {
        idx = SkipWhitespace(sql, idx);
        if (idx < sql.Length && sql[idx] == '(')
        {
            ReadParenthesizedBlock(sql, ref idx);
        }
    }

    private static string ReadParenthesizedBlock(string sql, ref int idx)
    {
        int depth = 0;
        int start = idx;
        if (idx >= sql.Length || sql[idx] != '(')
            throw new Exception("Parenthesized block expected.");

        while (idx < sql.Length)
        {
            var ch = sql[idx++];
            if (ch == '(') depth++;
            else if (ch == ')')
            {
                depth--;
                if (depth == 0) break;
            }
        }
        return sql.Substring(start, idx - start);
    }

    private static string ReadUntilKeywordsOrParenBalanced(string sql, ref int idx, string[] keywords)
    {
        int start = idx;
        int depth = 0;
        while (idx < sql.Length)
        {
            var ch = sql[idx];
            if (ch == '(') depth++;
            else if (ch == ')') depth = Math.Max(0, depth - 1);

            if (depth == 0)
            {
                foreach (var kw in keywords)
                {
                    if (StartsWithKeyword(sql, idx, kw))
                    {
                        var res = sql.Substring(start, idx - start).Trim();
                        return res;
                    }
                }
            }
            idx++;
        }
        return sql.Substring(start).Trim();
    }

    private static List<string> SplitTopLevel(string s, char sep)
    {
        var items = new List<string>();
        int depth = 0;
        int start = 0;
        for (int i = 0; i < s.Length; i++)
        {
            var ch = s[i];
            if (ch == '(') depth++;
            else if (ch == ')') depth--;
            else if (ch == sep && depth == 0)
            {
                items.Add(s.Substring(start, i - start));
                start = i + 1;
            }
        }
        items.Add(s.Substring(start));
        return items;
    }

    private static string ExtractParenthesized(string s)
    {
        int depth = 0;
        int i = 0;
        for (; i < s.Length; i++)
        {
            var ch = s[i];
            if (ch == '(') depth++;
            else if (ch == ')')
            {
                depth--;
                if (depth == 0)
                {
                    i++;
                    break;
                }
            }
        }
        return s.Substring(0, i);
    }

    private static List<string> TokenizeIdentifiers(string s)
    {
        var tokens = new List<string>();
        foreach (var part in Regex.Split(s, @"\s+"))
        {
            var p = part.Trim();
            if (!string.IsNullOrEmpty(p))
                tokens.Add(p);
        }
        return tokens;
    }
}

// ===== Metadata & resolution =====
public class PgMetadataProvider
{
    private readonly string _connStr;
    public PgMetadataProvider(string connectionString) { _connStr = connectionString; }

    public List<PgColumn> GetColumns(string schema, string table)
    {
        schema ??= "public";
        var cols = new List<PgColumn>();
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT c.column_name, c.data_type
            FROM information_schema.columns c
            WHERE c.table_schema = @schema AND c.table_name = @table
            ORDER BY c.ordinal_position;", conn);
        cmd.Parameters.AddWithValue("@schema", schema);
        cmd.Parameters.AddWithValue("@table", table);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            cols.Add(new PgColumn { ColumnName = r.GetString(0), DataType = r.GetString(1) });
        }
        return cols;
    }
}

public class PgColumn
{
    public string ColumnName { get; set; }
    public string DataType { get; set; }
}


public class ResolvedQuery
{
    public List<string> AccessedTables { get; set; } = new();
    public List<ResolvedColumn> OutputColumns { get; set; } = new();
}

public class ResolvedColumn
{
    public string OutputName { get; set; }
    public string SourceTable { get; set; }
    public string SourceAlias { get; set; }
    public string SourceColumn { get; set; }
    public string PgDataType { get; set; }
}

public class QueryResolver
{
    private readonly PgMetadataProvider _meta;

    public QueryResolver(PgMetadataProvider meta)
    {
        _meta = meta;
    }

    public ResolvedQuery Resolve(QueryNode node)
    {
        var result = new ResolvedQuery();

        // 1) Thu thập tất cả bảng truy cập (đi xuyên subquery)
        var tables = new List<TableRef>();
        CollectTables(node, tables);

        // 2) Alias map: alias -> TableRef để resolve cột
        var aliasMap = BuildAliasMap(node);

        // 3) Mở rộng SELECT items (kể cả wildcard *)
        var outputColumns = new List<ResolvedColumn>();
        foreach (var item in node.SelectItems)
        {
            switch (item)
            {
                case WildcardRef w:
                    if (w.TableAlias == null)
                    {
                        // Global * => mở rộng tất cả nguồn trong FROM (table)
                        foreach (var src in node.FromSources)
                        {
                            ExpandWildcardForSource(src, aliasMap, outputColumns);
                        }
                    }
                    else
                    {
                        // alias.* => mở rộng cột của bảng alias
                        if (!aliasMap.TryGetValue(w.TableAlias, out var tref))
                            throw new Exception($"Unknown table alias in wildcard: {w.TableAlias}");

                        var cols = _meta.GetColumns(tref.Schema, tref.Name);
                        foreach (var c in cols)
                        {
                            outputColumns.Add(new ResolvedColumn
                            {
                                OutputName = $"{w.TableAlias}.{c.ColumnName}",
                                SourceTable = $"{tref.Schema ?? "public"}.{tref.Name}",
                                SourceAlias = w.TableAlias,
                                SourceColumn = c.ColumnName,
                                PgDataType = c.DataType
                            });
                        }
                    }
                    break;

                case ColumnRef cr:
                    TableRef source = null;
                    string srcAlias = cr.TableAlias;

                    if (srcAlias != null)
                    {
                        if (!aliasMap.TryGetValue(srcAlias, out source))
                            throw new Exception($"Unknown table alias: {srcAlias}");
                    }
                    else
                    {
                        // Bare column: tìm bảng duy nhất sở hữu cột
                        source = FindUniqueColumnOwner(cr.ColumnName, aliasMap.Values.ToList());
                        srcAlias = source?.Alias;
                    }

                    if (source == null)
                        throw new Exception($"Cannot resolve column: {cr.ColumnName}");

                    var srcCols = _meta.GetColumns(source.Schema, source.Name);
                    var colMeta = srcCols.FirstOrDefault(x =>
                        x.ColumnName.Equals(cr.ColumnName, StringComparison.OrdinalIgnoreCase));
                    if (colMeta == null)
                        throw new Exception($"Column not found: {cr.ColumnName} in {source.Name}");

                    outputColumns.Add(new ResolvedColumn
                    {
                        OutputName = cr.Alias ?? (srcAlias != null ? $"{srcAlias}.{cr.ColumnName}" : cr.ColumnName),
                        SourceTable = $"{source.Schema ?? "public"}.{source.Name}",
                        SourceAlias = srcAlias,
                        SourceColumn = cr.ColumnName,
                        PgDataType = colMeta.DataType
                    });
                    break;

                case ExpressionRef er:
                    // Không suy loại được kiểu cho expression nếu không có rule → unknown
                    outputColumns.Add(new ResolvedColumn
                    {
                        OutputName = er.Alias ?? er.ExpressionSql,
                        SourceTable = null,
                        SourceAlias = null,
                        SourceColumn = null,
                        PgDataType = "unknown"
                    });
                    break;
            }
        }

        result.AccessedTables = tables
            .Select(t => $"{t.Schema ?? "public"}.{t.Name} AS {t.Alias}")
            .Distinct()
            .ToList();
        result.OutputColumns = outputColumns;
        return result;
    }

    private void ExpandWildcardForSource(FromSource src, Dictionary<string, TableRef> aliasMap, List<ResolvedColumn> output)
    {
        if (src is TableRef t)
        {
            var cols = _meta.GetColumns(t.Schema, t.Name);
            foreach (var c in cols)
            {
                output.Add(new ResolvedColumn
                {
                    OutputName = $"{t.Alias}.{c.ColumnName}",
                    SourceTable = $"{t.Schema ?? "public"}.{t.Name}",
                    SourceAlias = t.Alias,
                    SourceColumn = c.ColumnName,
                    PgDataType = c.DataType
                });
            }
        }
        else if (src is SubqueryRef sq)
        {
            // Có thể resolve đệ quy để lấy cột của subquery, tạm gán unknown ở bản đơn giản
            output.Add(new ResolvedColumn
            {
                OutputName = $"{sq.Alias}.*",
                SourceTable = null,
                SourceAlias = sq.Alias,
                SourceColumn = "*",
                PgDataType = "unknown"
            });
        }
    }

    private void CollectTables(QueryNode node, List<TableRef> acc)
    {
        foreach (var src in node.FromSources)
        {
            switch (src)
            {
                case TableRef t:
                    acc.Add(t);
                    break;
                case SubqueryRef sq:
                    CollectTables(sq.Subquery, acc);
                    break;
            }
        }
    }

    private Dictionary<string, TableRef> BuildAliasMap(QueryNode node)
    {
        var map = new Dictionary<string, TableRef>(StringComparer.OrdinalIgnoreCase);
        foreach (var src in node.FromSources)
        {
            if (src is TableRef t)
            {
                map[t.Alias] = t;
            }
            // SubqueryRef có alias nhưng không có schema/name thật; nếu muốn, có thể thêm vào map logic riêng.
        }
        return map;
    }

    private TableRef FindUniqueColumnOwner(string column, List<TableRef> tables)
    {
        var owners = new List<TableRef>();
        foreach (var t in tables)
        {
            var cols = _meta.GetColumns(t.Schema, t.Name);
            if (cols.Any(c => c.ColumnName.Equals(column, StringComparison.OrdinalIgnoreCase)))
            {
                owners.Add(t);
            }
        }

        if (owners.Count == 1) return owners[0];
        if (owners.Count == 0) return null;

        throw new Exception(
            $"Ambiguous column '{column}' appears in multiple tables: {string.Join(", ", owners.Select(o => o.Name))}");
    }
}
