using System.Data;
using CommonDb.DbCommands;
using CommonObject;
using Npgsql;
using NpgsqlTypes;
using ProtoBuf.WellKnownTypes;

namespace PostgresExtCm.Results;

public static class PostgresUtils
{
    private static readonly Dictionary<Type, DbType> TypeMap = new()
    {
        // (String Types)
        [typeof(string)] = DbType.String,
        [typeof(char[])] = DbType.StringFixedLength,
        [typeof(char)] = DbType.StringFixedLength,
        // (Integer Types) 
        [typeof(byte)] = DbType.Byte,
        [typeof(sbyte)] = DbType.SByte,
        [typeof(short)] = DbType.Int16,
        [typeof(ushort)] = DbType.UInt16,
        [typeof(int)] = DbType.Int32,
        [typeof(uint)] = DbType.UInt32,
        [typeof(long)] = DbType.Int64,
        [typeof(ulong)] = DbType.UInt64,
        // (Floating-Point & Currency Types) 
        [typeof(float)] = DbType.Single,
        [typeof(double)] = DbType.Double,
        [typeof(decimal)] = DbType.Decimal,
        // Date & Time
        [typeof(DateTime)] = DbType.DateTime,
        [typeof(Timestamp)] = DbType.DateTime,
        [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
        [typeof(TimeSpan)] = DbType.Time,
        [typeof(DateOnly)] = DbType.Date,
        [typeof(TimeOnly)] = DbType.Time,
        // (Logical & Identifier Types) 
        [typeof(bool)] = DbType.Boolean,
        [typeof(Guid)] = DbType.Guid,
        // (Binary & Special Types) 
        [typeof(byte[])] = DbType.Binary,
        [typeof(object)] = DbType.Object,
        // XML 
        [typeof(System.Xml.Linq.XDocument)] = DbType.Xml,
        [typeof(System.Xml.XmlDocument)] = DbType.Xml,
        // json 
        [typeof(Newtonsoft.Json.Linq.JObject)] = DbType.String,
        [typeof(Newtonsoft.Json.Linq.JArray)] = DbType.String,
        [typeof(System.Text.Json.JsonDocument)] = DbType.String,
        [typeof(Newtonsoft.Json.Linq.JToken)] = DbType.String,
    };

    private static readonly Dictionary<Type, NpgsqlDbType> NpgsqlTypeMap = new()
    {
        // json
        [typeof(Newtonsoft.Json.Linq.JObject)] = NpgsqlDbType.Json,
        [typeof(Newtonsoft.Json.Linq.JArray)] = NpgsqlDbType.Json,
        [typeof(System.Text.Json.JsonDocument)] = NpgsqlDbType.Json,
        [typeof(Newtonsoft.Json.Linq.JToken)] = NpgsqlDbType.Json,
    };

    public static NpgsqlDbType? ToNpgsqlDbType(this Type type)
    {
        return NpgsqlTypeMap.GetValueOrDefault(type);
    }

    public static DbType ToDbType(this Type type)
    {
        var nonNullableType = Nullable.GetUnderlyingType(type) ?? type;
        if (nonNullableType.IsEnum)
        {
            return DbType.Int32;
        }

        return TypeMap.GetValueOrDefault(nonNullableType, DbType.Object);
    }

    private static object? ConvertArrayElement(string elementString, NpgsqlDbType elementType)
    {
        return elementType switch
        {
            NpgsqlDbType.Smallint => short.TryParse(elementString, out var s) ? s : null,
            NpgsqlDbType.Integer => int.TryParse(elementString, out var i) ? i : null,
            NpgsqlDbType.Bigint => long.TryParse(elementString, out var l) ? l : null,
            NpgsqlDbType.Real => float.TryParse(elementString, out var f) ? f : null,
            NpgsqlDbType.Double => double.TryParse(elementString, out var d) ? d : null,
            NpgsqlDbType.Numeric => decimal.TryParse(elementString, out var dec) ? dec : null,
            NpgsqlDbType.Boolean => bool.TryParse(elementString, out var b)
                ? b
                : (elementString == "1" ? true : elementString == "0" ? false : null),
            NpgsqlDbType.Date or NpgsqlDbType.Timestamp or NpgsqlDbType.TimestampTz => DateTime.TryParse(elementString,
                out var dt)
                ? dt
                : null,
            NpgsqlDbType.Uuid => Guid.TryParse(elementString, out var g) ? g : null,
            // Thêm các kiểu khác nếu cần (Jsonb, Text, v.v.)
            _ => elementString
        };
    }

    public static object? ConvertStringValue(this string stringValue, NpgsqlDbType targetType)
    {
        if (string.IsNullOrEmpty(stringValue))
        {
            return null;
        }

        // --- Xử lý Kiểu Mảng ---
        if (targetType.HasFlag(NpgsqlDbType.Array))
        {
            var elementType = targetType & ~NpgsqlDbType.Array;
            var elements = stringValue.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            if (!elements.Any()) return Array.Empty<object>();

            var convertedElements = elements
                .Select(element => ConvertArrayElement(element, elementType))
                .ToArray();
            return convertedElements;
        }

        // --- Xử lý Kiểu Đơn Lẻ (Scalar Types) ---
        return targetType switch
        {
            // Số nguyên
            NpgsqlDbType.Smallint => short.TryParse(stringValue, out var s) ? s : null,
            NpgsqlDbType.Integer => int.TryParse(stringValue, out var i) ? i : null,
            NpgsqlDbType.Bigint => long.TryParse(stringValue, out var l) ? l : null,

            // Số thực
            NpgsqlDbType.Real => float.TryParse(stringValue, out var f) ? f : null,
            NpgsqlDbType.Double => double.TryParse(stringValue, out var d) ? d : null,
            NpgsqlDbType.Numeric => decimal.TryParse(stringValue, out var dec) ? dec : null,

            // Boolean
            NpgsqlDbType.Boolean => bool.TryParse(stringValue, out var b)
                ? b
                : stringValue.Trim().Equals("1")
                    ? true
                    : stringValue.Trim().Equals("0")
                        ? false
                        : null,

            // Ngày giờ
            NpgsqlDbType.Date or NpgsqlDbType.Timestamp => DateTime.TryParse(stringValue, out var dt) ? dt : null,

            // Timestamp with Timezone (GMT/UTC)
            NpgsqlDbType.TimestampTz => DateTime.TryParse(stringValue, null,
                System.Globalization.DateTimeStyles.AdjustToUniversal |
                System.Globalization.DateTimeStyles.AssumeUniversal, out var dtUtc)
                ? dtUtc
                : null,

            // Guid
            NpgsqlDbType.Uuid => Guid.TryParse(stringValue, out var g) ? g : null,
            NpgsqlDbType.Json or NpgsqlDbType.Jsonb => stringValue, // string
            _ => stringValue
        };
    }

    public static NpgsqlParameter CreateNpgsqlParameter(this NpOnDbCommandParam<NpgsqlDbType> npgsqlParam)
    {
        var paramValue = npgsqlParam.ParamValue;
        var paramType = npgsqlParam.ParamType;

        if (paramValue is string stringValue)
            paramValue = stringValue.ConvertStringValue(paramType);

        // Bắt buộc chuyển đổi sang UTC nếu là DateTime và loại tham số là TimestampTz
        if (paramValue is DateTime dt && paramType == NpgsqlDbType.TimestampTz)
        {
            // Nếu Kind là Unspecified, coi nó là giờ Local và chuyển sang UTC.
            paramValue = dt.ToUniversalTime();
        }

        return new NpgsqlParameter(npgsqlParam.ParamName.AsDefaultString(), paramType)
        {
            Value = paramValue ?? DBNull.Value
        };
    }
}