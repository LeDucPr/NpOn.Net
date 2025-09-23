using CommonDb.DbCommands;
using NpgsqlTypes;
using System.Data;

namespace PostgresExtCm.Sql;

public class PostgresTile<T> : NpOnTile<T>
{
    private readonly NpgsqlDbType? _npgsqlDbType;

    public PostgresTile(T? value, NpgsqlDbType? npgsqlDbType) : base()
    {
        _npgsqlDbType = npgsqlDbType;
        DbTileType = npgsqlDbType?.ToDbType();
        Value = value;
    }
}

public static class NpgsqlDbTypeExtensions
{
    public static NpgsqlDbType? ToNpgsqlDbType(this DbType dbType)
        => dbType switch
        {
            DbType.AnsiString => NpgsqlDbType.Text,
            DbType.Binary => NpgsqlDbType.Bytea,
            DbType.Byte => NpgsqlDbType.Smallint,
            DbType.Boolean => NpgsqlDbType.Boolean,
            DbType.Currency => NpgsqlDbType.Money,
            DbType.Date => NpgsqlDbType.Date,
            DbType.DateTime => LegacyTimestampBehavior ? NpgsqlDbType.Timestamp : NpgsqlDbType.TimestampTz,
            DbType.Decimal => NpgsqlDbType.Numeric,
            DbType.VarNumeric => NpgsqlDbType.Numeric,
            DbType.Double => NpgsqlDbType.Double,
            DbType.Guid => NpgsqlDbType.Uuid,
            DbType.Int16 => NpgsqlDbType.Smallint,
            DbType.Int32 => NpgsqlDbType.Integer,
            DbType.Int64 => NpgsqlDbType.Bigint,
            DbType.Single => NpgsqlDbType.Real,
            DbType.String => NpgsqlDbType.Text,
            DbType.Time => NpgsqlDbType.Time,
            DbType.AnsiStringFixedLength => NpgsqlDbType.Text,
            DbType.StringFixedLength => NpgsqlDbType.Text,
            DbType.Xml => NpgsqlDbType.Xml,
            DbType.DateTime2 => NpgsqlDbType.Timestamp,
            DbType.DateTimeOffset => NpgsqlDbType.TimestampTz,

            DbType.Object => null,
            DbType.SByte => null,
            DbType.UInt16 => null,
            DbType.UInt32 => null,
            DbType.UInt64 => null,

            _ => throw new ArgumentOutOfRangeException(nameof(dbType), dbType, null)
        };

    public static bool LegacyTimestampBehavior { get; set; }

    public static DbType ToDbType(this NpgsqlDbType npgsqlDbType)
        => npgsqlDbType switch
        {
            // Numeric types
            NpgsqlDbType.Smallint => DbType.Int16,
            NpgsqlDbType.Integer => DbType.Int32,
            NpgsqlDbType.Bigint => DbType.Int64,
            NpgsqlDbType.Real => DbType.Single,
            NpgsqlDbType.Double => DbType.Double,
            NpgsqlDbType.Numeric => DbType.Decimal,
            NpgsqlDbType.Money => DbType.Currency,

            // Text types
            NpgsqlDbType.Text => DbType.String,
            NpgsqlDbType.Xml => DbType.Xml,
            NpgsqlDbType.Varchar => DbType.String,
            NpgsqlDbType.Char => DbType.String,
            NpgsqlDbType.Name => DbType.String,
            NpgsqlDbType.Citext => DbType.String,
            NpgsqlDbType.Refcursor => DbType.Object,
            NpgsqlDbType.Jsonb => DbType.Object,
            NpgsqlDbType.Json => DbType.Object,
            NpgsqlDbType.JsonPath => DbType.Object,

            // Date/time types
            NpgsqlDbType.Timestamp => LegacyTimestampBehavior ? DbType.DateTime : DbType.DateTime2,
            NpgsqlDbType.TimestampTz => LegacyTimestampBehavior ? DbType.DateTimeOffset : DbType.DateTime,
            NpgsqlDbType.Date => DbType.Date,
            NpgsqlDbType.Time => DbType.Time,

            // Misc data types
            NpgsqlDbType.Bytea => DbType.Binary,
            NpgsqlDbType.Boolean => DbType.Boolean,
            NpgsqlDbType.Uuid => DbType.Guid,

            NpgsqlDbType.Unknown => DbType.Object,

            _ => DbType.Object
        };
}