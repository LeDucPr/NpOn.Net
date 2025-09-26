using System.Data;

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
    };

    public static DbType ToDbType(this Type type)
    {
        var nonNullableType = Nullable.GetUnderlyingType(type) ?? type;
        if (nonNullableType.IsEnum)
        {
            return DbType.Int32;
        }
        return TypeMap.GetValueOrDefault(nonNullableType, DbType.Object);
    }
}