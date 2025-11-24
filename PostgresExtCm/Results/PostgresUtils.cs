﻿using System.Data;
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
    
    private static object? ConvertStringValue(this string stringValue, NpgsqlDbType targetType)
    {
        if (string.IsNullOrEmpty(stringValue))
        {
            return null;
        }

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
                : (stringValue == "1" ? true : stringValue == "0" ? false : null),

            // Ngày giờ
            NpgsqlDbType.Date or NpgsqlDbType.Timestamp => DateTime.TryParse(stringValue, out var dt) ? dt : null,
            NpgsqlDbType.TimestampTz => DateTime.TryParse(stringValue, null,
                System.Globalization.DateTimeStyles.AdjustToUniversal, out var dtUtc)
                ? dtUtc
                : null,

            // Guid
            NpgsqlDbType.Uuid => Guid.TryParse(stringValue, out var g) ? g : null,
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