namespace CommonDb.DbCommands;

using System.Data;

public interface INpOnTile
{
    
}

public interface INpOnTile<out T> : INpOnTile
{
    public DbType? DbTileType { get; }
    public Type? StaticTileType { get; }
    T? Value { get; }
}

public class NpOnTile<T> : INpOnTile<T>
{
    public NpOnTile(T? value, Type? staticTileType)
    {
        Value = value;
        StaticTileType = staticTileType;
        DbTileType = staticTileType?.ToDbType();
    }

    public NpOnTile(T? value, DbType? dbTileType)
    {
        Value = value;
        DbTileType = dbTileType;
        StaticTileType = dbTileType?.ToSystemType();
    }

    public NpOnTile(){}
    public DbType? DbTileType { get; protected set; }

    public Type? StaticTileType { get; protected set; }

    public T? Value { get; protected set; }
}

public static class TypeExtensions
{
    private static readonly Dictionary<Type, DbType> TypeMap = new()
    {
        [typeof(string)] = DbType.String,
        [typeof(char[])] = DbType.StringFixedLength,
        [typeof(byte)] = DbType.Byte,
        [typeof(sbyte)] = DbType.SByte,
        [typeof(short)] = DbType.Int16,
        [typeof(ushort)] = DbType.UInt16,
        [typeof(int)] = DbType.Int32,
        [typeof(uint)] = DbType.UInt32,
        [typeof(long)] = DbType.Int64,
        [typeof(ulong)] = DbType.UInt64,
        [typeof(float)] = DbType.Single,
        [typeof(double)] = DbType.Double,
        [typeof(decimal)] = DbType.Decimal,
        [typeof(DateTime)] = DbType.DateTime,
        [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
        [typeof(TimeSpan)] = DbType.Time,
        [typeof(DateOnly)] = DbType.Date,
        [typeof(TimeOnly)] = DbType.Time,
        [typeof(bool)] = DbType.Boolean,
        [typeof(Guid)] = DbType.Guid,
        [typeof(byte[])] = DbType.Binary,
        [typeof(object)] = DbType.Object,
        [typeof(System.Xml.Linq.XDocument)] = DbType.Xml,
        [typeof(System.Xml.XmlDocument)] = DbType.Xml,
    };

    private static readonly Dictionary<DbType, Type> DbTypeMap = new()
    {
        // --- String Types ---
        [DbType.String] = typeof(string),
        [DbType.AnsiString] = typeof(string),
        [DbType.StringFixedLength] = typeof(string),
        [DbType.AnsiStringFixedLength] = typeof(string),
        // --- Integer Types ---
        [DbType.Byte] = typeof(byte),
        [DbType.SByte] = typeof(sbyte),
        [DbType.Int16] = typeof(short),
        [DbType.UInt16] = typeof(ushort),
        [DbType.Int32] = typeof(int),
        [DbType.UInt32] = typeof(uint),
        [DbType.Int64] = typeof(long),
        [DbType.UInt64] = typeof(ulong),
        // --- Floating-Point & Currency Types ---
        [DbType.Single] = typeof(float),
        [DbType.Double] = typeof(double),
        [DbType.Decimal] = typeof(decimal),
        [DbType.Currency] = typeof(decimal),
        // --- Date/Time Types ---
        [DbType.DateTime] = typeof(DateTime),
        [DbType.DateTime2] = typeof(DateTime),
        [DbType.Date] = typeof(DateOnly),
        [DbType.Time] = typeof(TimeOnly),
        [DbType.DateTimeOffset] = typeof(DateTimeOffset),
        // --- Logical & Identifier Types ---
        [DbType.Boolean] = typeof(bool),
        [DbType.Guid] = typeof(Guid),
        // --- Binary & Special Types ---
        [DbType.Binary] = typeof(byte[]),
        [DbType.Object] = typeof(object),
        [DbType.Xml] = typeof(string),
    };

    public static DbType ToDbType(this Type type)
    {
        var nonNullableType = Nullable.GetUnderlyingType(type) ?? type;
        return TypeMap.GetValueOrDefault(nonNullableType, DbType.Object);
    }

    public static Type ToSystemType(this DbType dbType)
        => DbTypeMap.GetValueOrDefault(dbType, typeof(object));
}