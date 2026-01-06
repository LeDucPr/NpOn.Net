using System.Text.RegularExpressions;

namespace Tools.PgGenFldCode.Object;

public static class SqlParamExtractor
{
    private static readonly Regex ParamRegex =
        new(@"@([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

    public static HashSet<string> Extract(string sql)
    {
        return ParamRegex.Matches(sql)
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}