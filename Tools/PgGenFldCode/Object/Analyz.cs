using System.Text;
using MicroServices.General.Contract.GeneralServiceContract.Domains;
using NpgsqlTypes;
using Tools.PgGenFldCode.Parser;

namespace Tools.PgGenFldCode.Object;

public class AnalyzeResultDomain
{
    public TblMaster? TblMaster { get; set; }
    public List<FldQueryMaster>? Fields { get; set; } = new();
    public string? ReadModelCode { get; set; }
    public string? DomainCode { get; set; }
    public string? ExecutionCode { get; set; }
}

public class Analyz
{
    // public async Task<(AnalyzeResultDomain? result, string message, bool status)>
    //     AnalyzeToDomain(string execString, string resourceConnectString, TblMaster baseTbl)
    // {
    //     if (string.IsNullOrWhiteSpace(execString))
    //         return (null, "SQL string is empty", false);
    //
    //     try
    //     {
    //         // 1. Parse + Resolve
    //         var parser = new SqlParser(execString);
    //         var node = parser.Parse();
    //
    //         var meta = new PgMetadataProvider(resourceConnectString);
    //         var resolver = new QueryResolver(meta);
    //         var resolved = resolver.Resolve(node);
    //
    //         // 2. Build TblMaster
    //         var tblMaster = new TblMaster
    //         {
    //             Id = baseTbl.Id == Guid.Empty ? Guid.NewGuid() : baseTbl.Id,
    //             Code = baseTbl.Code,
    //             Description = baseTbl.Description,
    //             ExecFunc = baseTbl.ExecFunc,
    //             Query = execString,
    //             ExecType = baseTbl.ExecType,
    //             ServiceName = baseTbl.ServiceName,
    //             DbType = baseTbl.DbType,
    //             CreatedAt = DateTime.UtcNow
    //         };
    //
    //         // 3. Build FldQueryMaster list
    //         int order = 0;
    //         var fields = new List<FldQueryMaster>();
    //
    //         foreach (var col in resolved.OutputColumns)
    //         {
    //             var npgsqlType = PgTypeMapper.Map(col.PgDataType);
    //
    //             fields.Add(new FldQueryMaster
    //             {
    //                 Id = Guid.NewGuid(),
    //                 TblMasterId = tblMaster.Id,
    //                 Description = col.OutputName,
    //                 FieldName = col.OutputName,
    //                 FieldType = (int)npgsqlType,
    //                 FieldTypeString = $"NpgsqlDbType.{npgsqlType}",
    //                 OrderSort = order++,
    //                 CreatedAt = DateTime.UtcNow
    //             });
    //         }
    //
    //         return (new AnalyzeResultDomain
    //         {
    //             TblMaster = tblMaster,
    //             Fields = fields
    //         }, "Analyze success", true);
    //     }
    //     catch (Exception ex)
    //     {
    //         return (null, ex.Message, false);
    //     }
    // }
    //

    public async Task<(AnalyzeResultDomain? result, string message, bool status)>
        AnalyzeToDomain(string execString, string resourceConnectString, TblMaster baseTbl)
    {
        if (string.IsNullOrWhiteSpace(execString))
            return (null, "SQL string is empty", false);

        try
        {
            // 1. Parse + resolve
            var parser = new SqlParser(execString);
            var node = parser.Parse();

            var meta = new PgMetadataProvider(resourceConnectString);
            var queryResolver = new QueryResolver(meta);
            var resolved = queryResolver.Resolve(node);

            // 2. TblMaster
            var tblMaster = new TblMaster
            {
                Id = (baseTbl.Id == null || baseTbl.Id == Guid.Empty) ? Guid.NewGuid() : baseTbl.Id,
                Code = baseTbl.Code,
                Description = baseTbl.Description,
                ExecFunc = baseTbl.ExecFunc,
                Query = execString,
                ExecType = baseTbl.ExecType,
                ServiceName = baseTbl.ServiceName,
                DbType = baseTbl.DbType,
                CreatedAt = DateTime.UtcNow
            };

            // 3. PARAMETER resolver
            var paramTypeResolver = new PgParamTypeResolver(meta);

            // 4. Extract @params
            var paramNames = SqlParamExtractor.Extract(execString);

            // 5. Build FldQueryMaster (INPUT PARAM)
            int order = 0;
            var fields = new List<FldQueryMaster>();

            foreach (var param in paramNames)
            {
                var npgsqlType = paramTypeResolver.Resolve(
                    param,
                    execString,
                    resolved.TableAliases);

                fields.Add(new FldQueryMaster
                {
                    Id = Guid.NewGuid(),
                    TblMasterId = tblMaster.Id ?? Guid.Empty,
                    FieldName = param,
                    Description = $"Parameter @{param}",
                    FieldType = (int)npgsqlType,
                    FieldTypeString = Enum.IsDefined(typeof(NpgsqlDbType), npgsqlType) ? $"NpgsqlDbType.{npgsqlType}" : null,
                    OrderSort = order++,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 6. Generate ReadModel Code (Output Columns) - Separate StringBuilder
            var sb = new StringBuilder();
            var sbDomain = new StringBuilder();
            var fieldMapSb = new StringBuilder();
            var seenProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int protoIndex = 1;
            string className = string.IsNullOrWhiteSpace(baseTbl.Code) ? "Generated" : baseTbl.Code;

            string tableName = "unknown_table";
            if (resolved.AccessedTables != null && resolved.AccessedTables.Count > 0)
            {
                var firstTable = resolved.AccessedTables[0];
                var parts = firstTable.Split(new[] { " AS " }, StringSplitOptions.RemoveEmptyEntries);
                var schemaTable = parts[0];
                tableName = schemaTable.Contains('.') ? schemaTable.Split('.')[1] : schemaTable;
            }

            sbDomain.AppendLine("[ProtoContract]");
            sbDomain.AppendLine($"[TableLoader(\"{tableName}\")]");
            sbDomain.AppendLine($"public class {className} : DOMAIN");
            sbDomain.AppendLine("{");

            foreach (var col in resolved.OutputColumns)
            {
                var pascalName = ToPascalCase(col.OutputName);
                if (seenProps.Contains(pascalName)) continue;
                seenProps.Add(pascalName);

                var npgsqlType = PgTypeMapper.Map(col.PgDataType);
                var csharpType = GetCSharpType(npgsqlType);

                sb.AppendLine($"    [ProtoMember({protoIndex})] public {csharpType} {pascalName} {{ get; set; }}");

                var cleanColName = col.OutputName;
                if (cleanColName.Contains('.'))
                    cleanColName = cleanColName.Substring(cleanColName.LastIndexOf('.') + 1);

                sbDomain.AppendLine($"    [ProtoMember({protoIndex})] [Field(\"{cleanColName}\")] public {csharpType} {pascalName} {{ get; set; }}");

                // Build FieldMap content
                fieldMapSb.AppendLine($"        FieldMap.Add(nameof({pascalName}), \"{cleanColName}\");");
                protoIndex++;
            }

            sb.AppendLine("");
            sb.AppendLine("    protected override void FieldMapper()");
            sb.AppendLine("    {");
            sb.AppendLine("        FieldMap ??= new();");
            sb.Append(fieldMapSb.ToString());
            sb.AppendLine("    }");

            sbDomain.AppendLine("}");

            // 7. Generate Execution Code
            var sbExec = new StringBuilder();
            sbExec.AppendLine("var execution = new TblFldExecution");
            sbExec.AppendLine("{");
            sbExec.AppendLine($"    Code = {baseTbl.Code},");
            sbExec.AppendLine("    ExecParams =");
            sbExec.AppendLine("    [");
            foreach (var param in paramNames)
            {
                var npgsqlType = paramTypeResolver.Resolve(param, execString, resolved.TableAliases);
                var propName = ToPascalCase(param);
                var valExpr = GetValueExpression("request", propName, npgsqlType);

                sbExec.AppendLine("        new TblFldExecutionParam");
                sbExec.AppendLine("        {");
                sbExec.AppendLine($"            ParamName = \"{param}\",");
                sbExec.AppendLine($"            StringValue = {valExpr}");
                sbExec.AppendLine("        },");
            }
            sbExec.AppendLine("    ]");
            sbExec.AppendLine("};");

            return (new AnalyzeResultDomain
            {
                TblMaster = tblMaster,
                Fields = fields,
                ReadModelCode = sb.ToString(),
                DomainCode = sbDomain.ToString(),
                ExecutionCode = sbExec.ToString()
            }, "Analyze success", true);
        }
        catch (Exception ex)
        {
            return (null, ex.Message, false);
        }
    }

    private static string ToPascalCase(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;

        if (str.Contains('.')) // Handle table alias prefixes like "e.id" -> "id"
            str = str.Substring(str.LastIndexOf('.') + 1);

        return string.Join("", str.Split(new[]
            {
                '_',
                ' '
            }, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant()));
    }

    private static string GetCSharpType(NpgsqlDbType type)
    {
        return type switch
        {
            NpgsqlDbType.Bigint => "long",
            NpgsqlDbType.Integer => "int",
            NpgsqlDbType.Smallint => "short",
            NpgsqlDbType.Boolean => "bool",
            NpgsqlDbType.Double => "double",
            NpgsqlDbType.Real => "float",
            NpgsqlDbType.Numeric => "decimal",
            NpgsqlDbType.Money => "decimal",
            NpgsqlDbType.Text => "string",
            NpgsqlDbType.Varchar => "string",
            NpgsqlDbType.Char => "string",
            NpgsqlDbType.Xml => "string",
            NpgsqlDbType.Json => "string",
            NpgsqlDbType.Jsonb => "string",
            NpgsqlDbType.Date => "DateTime?",
            NpgsqlDbType.Timestamp => "DateTime?",
            NpgsqlDbType.TimestampTz => "DateTime?",
            NpgsqlDbType.Time => "TimeSpan?",
            NpgsqlDbType.Uuid => "Guid",
            _ => "string" // Default fallback
        };
    }

    private static string GetValueExpression(string objName, string propName, NpgsqlDbType type)
    {
        if ((type & NpgsqlDbType.Array) == NpgsqlDbType.Array)
            return $"{objName}.{propName}.AsArrayJoin()";

        return type switch
        {
            NpgsqlDbType.Text or NpgsqlDbType.Varchar or NpgsqlDbType.Char or NpgsqlDbType.Xml or NpgsqlDbType.Json or NpgsqlDbType.Jsonb => $"{objName}.{propName}",
            _ => $"{objName}.{propName}.AsDefaultString()"
        };
    }
}