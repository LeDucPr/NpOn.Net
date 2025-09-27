using System.Data;
using MongoDB.Bson;

namespace MongoDbExtCm.Results;

public static class MongoDbUtils
{
    public static string GetBsonTypeName(BsonType bsonType) => bsonType.ToString();

    public static Type GetDotNetType(BsonType bsonType) => bsonType switch
    {
        BsonType.Double => typeof(double),
        BsonType.String => typeof(string),
        BsonType.ObjectId => typeof(ObjectId),
        BsonType.Boolean => typeof(bool),
        BsonType.DateTime => typeof(DateTime),
        BsonType.Int32 => typeof(int),
        BsonType.Int64 => typeof(long),
        BsonType.Decimal128 => typeof(decimal),
        BsonType.Binary => typeof(byte[]),
        BsonType.Document => typeof(MongoResultSetWrapper),
        BsonType.Array => typeof(MongoResultSetWrapper),
        _ => typeof(object) 
    };
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
        return nonNullableType.IsEnum ? DbType.Int32 : TypeMap.GetValueOrDefault(nonNullableType, DbType.Object);
    }
}