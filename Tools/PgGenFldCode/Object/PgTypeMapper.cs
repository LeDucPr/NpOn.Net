using NpgsqlTypes;

namespace Tools.PgGenFldCode.Object;

public static class PgTypeMapper
{
    public static NpgsqlDbType Map(string? dataType, string? udtName = null)
    {
        dataType = dataType?.ToLowerInvariant();
        udtName = udtName?.ToLowerInvariant();

        bool isArray =
            dataType == "array" ||
            (udtName != null && udtName.StartsWith("_"));

        // Base type
        var baseType = MapBaseType(dataType, udtName);

        return isArray
            ? baseType | NpgsqlDbType.Array
            : baseType;
    }

    private static NpgsqlDbType MapBaseType(string dataType, string? udtName)
    {
        // Ưu tiên udt_name nếu là array (_int4, _uuid...)
        if (udtName != null && udtName.StartsWith("_"))
        {
            udtName = udtName.Substring(1);
        }

        var t = (udtName ?? dataType).ToLowerInvariant();

        return t switch
        {
            "uuid" => NpgsqlDbType.Uuid,
            "int4" or "integer" => NpgsqlDbType.Integer,
            "int8" or "bigint" => NpgsqlDbType.Bigint,
            "int2" or "smallint" => NpgsqlDbType.Smallint,
            "bool" or "boolean" => NpgsqlDbType.Boolean,
            "text" => NpgsqlDbType.Text,
            "varchar" or "character varying" => NpgsqlDbType.Varchar,
            "timestamptz" or "timestamp with time zone" => NpgsqlDbType.TimestampTz,
            "timestamp" or "timestamp without time zone" => NpgsqlDbType.Timestamp,
            "date" => NpgsqlDbType.Date,
            "numeric" or "decimal" => NpgsqlDbType.Numeric,
            "float8" or "double precision" => NpgsqlDbType.Double,
            "float4" or "real" => NpgsqlDbType.Real,
            _ => NpgsqlDbType.Unknown
        };
    }
}