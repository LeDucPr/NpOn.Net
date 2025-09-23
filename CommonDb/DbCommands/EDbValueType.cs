using System.Collections.Concurrent;
using System.Reflection;

namespace CommonDb.DbCommands;

public enum EDbValueType : byte
{
    [EDbValue(EDb.Postgres, EDb.Cassandra, EDb.ScyllaDb)]
    Text = 1,

    [EDbValue(EDb.MongoDb)] Bson = 2,

    [EDbValue(EDb.Cassandra, EDb.ScyllaDb, EDb.Postgres, EDb.MongoDb)]
    Json = 3,

    [EDbValue(EDb.Cassandra, EDb.ScyllaDb, EDb.Postgres, EDb.MongoDb)]
    Array = 4,

    [EDbValue(EDb.Cassandra, EDb.ScyllaDb, EDb.Postgres)]
    DateTime = 5,

    [EDbValue(EDb.Cassandra, EDb.ScyllaDb, EDb.Postgres)]
    Number = 6,

    [EDbValue(EDb.Cassandra, EDb.ScyllaDb, EDb.Postgres)]
    Float = 7,

    [EDbValue(EDb.Cassandra, EDb.ScyllaDb, EDb.Postgres)]
    Integer = 8,
}

public class EDbValueAttribute : Attribute
{
    public IReadOnlyCollection<EDb> SupportedDbTypes { get; }

    public EDbValueAttribute(params EDb[] supportedDbTypes)
    {
        if (supportedDbTypes == null || supportedDbTypes.Length == 0)
        {
            throw new ArgumentException("Database does not support this data type", nameof(supportedDbTypes));
        }

        SupportedDbTypes = supportedDbTypes;
    }
}

public static class EDbValueTypeExtensions
{
    private static readonly ConcurrentDictionary<EDbValueType, IReadOnlyCollection<EDb>> Cache = new();

    public static bool IsValidFor(this EDbValueType valueType, EDb dbType)
    {
        // (từ cache hoặc qua Reflection)
        var supportedDbs = Cache.GetOrAdd(valueType, (v) =>
        {
            var memberInfo = typeof(EDbValueType).GetMember(v.ToString()).FirstOrDefault();
            if (memberInfo == null)
                return [];
            var attribute = memberInfo.GetCustomAttribute<EDbValueAttribute>();
            return attribute?.SupportedDbTypes ?? [];
        });

        return supportedDbs.Contains(dbType);
    }
}