using System.Data;
using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;
using Npgsql;
using NpgsqlTypes;

namespace Common.Infrastructures.NpOn.PostgresExtCm.Results;

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
        // [typeof(Timestamp)] = DbType.DateTime,
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

    /// <summary>
    /// Converts a raw value to a format suitable for an NpgsqlParameter,
    /// returning the normalized value and the corresponding NpgsqlDbType.
    /// </summary>
    /// <param name="raw">The raw input value.</param>
    /// <param name="memberType">The original type of the member.</param>
    /// <returns>A tuple containing the normalized value and its NpgsqlDbType.</returns>
    public static (object? Value, NpgsqlDbType? DbType) NormalizeValueForNpgsql(object raw, Type memberType)
    {
        var t = Nullable.GetUnderlyingType(memberType) ?? memberType;
        if (t.IsEnum)
        {
            var underlying = Enum.GetUnderlyingType(t);
            if (underlying == typeof(byte))
                return (Convert.ToByte(raw), NpgsqlDbType.Smallint);
            if (underlying == typeof(short))
                return (Convert.ToInt16(raw), NpgsqlDbType.Smallint);
            if (underlying == typeof(int))
                return (Convert.ToInt32(raw), NpgsqlDbType.Integer);
            if (underlying == typeof(long))
                return (Convert.ToInt64(raw), NpgsqlDbType.Bigint);
            return (Convert.ToInt64(raw), NpgsqlDbType.Bigint);
        }

        if (t == typeof(Guid)) return (raw, NpgsqlDbType.Uuid);
        if (t == typeof(DateTime)) return (raw, NpgsqlDbType.Timestamp);
        if (t == typeof(string)) return (raw, NpgsqlDbType.Text);
        if (t == typeof(int)) return (raw, NpgsqlDbType.Integer);
        if (t == typeof(long)) return (raw, NpgsqlDbType.Bigint);
        if (t == typeof(bool)) return (raw, NpgsqlDbType.Boolean);
        if (t == typeof(decimal)) return (raw, NpgsqlDbType.Numeric);
        if (t == typeof(double)) return (raw, NpgsqlDbType.Double);
        if (t == typeof(float)) return (raw, NpgsqlDbType.Real);
        if (NpgsqlTypeMap.TryGetValue(t, out var npgsqlDbType))
            return (raw.ToString(), npgsqlDbType); // Serialize JSON objects to string for the parameter

        return (raw, null);
    }
}