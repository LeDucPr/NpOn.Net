using System.Net;
using System.Numerics;

namespace CassandraExtCm.Results;

public static class CassandraUtils
{
    private static readonly IReadOnlyDictionary<Type, string> TypeToCqlMap = new Dictionary<Type, string>
    {
        // Kiểu chuỗi 
        [typeof(string)] = "text", // "varchar", "ascii"
        [typeof(char)] = "text",

        // Number 
        [typeof(int)] = "int",
        [typeof(long)] = "bigint",
        [typeof(short)] = "smallint",
        [typeof(byte)] = "tinyint",
        [typeof(BigInteger)] = "varint",
        [typeof(decimal)] = "decimal",
        [typeof(double)] = "double",
        [typeof(float)] = "float",
        // Time
        [typeof(DateTime)] = "timestamp",
        [typeof(DateTimeOffset)] = "timestamp",
        [typeof(TimeSpan)] = "duration", // Cassandra 4.0+
        // uuid & logic 
        [typeof(Guid)] = "uuid",
        [typeof(bool)] = "boolean",
        // binary & inet 
        [typeof(byte[])] = "blob",
        [typeof(IPAddress)] = "inet"
    };

    public static string GetCqlTypeName(Type type)
    {
        if (TypeToCqlMap.TryGetValue(type, out var cqlType))
        {
            return cqlType;
        }
        else if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(IDictionary<,>) || genericTypeDefinition == typeof(Dictionary<,>))
                return "map";
            if (genericTypeDefinition == typeof(ISet<>) || genericTypeDefinition == typeof(HashSet<>))
                return "set";
            if (genericTypeDefinition == typeof(IEnumerable<>) || genericTypeDefinition == typeof(IList<>) || genericTypeDefinition == typeof(List<>))
                return "list";
        }
        return "blob";
    }
}