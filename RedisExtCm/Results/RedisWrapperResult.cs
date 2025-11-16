using CommonDb;
using CommonDb.DbResults;
using Enums;
using System.Collections;
using StackExchange.Redis;

namespace RedisExtCm.Results;

/// <summary>
/// A container class to wrap the RedisValue struct, allowing it to be used as a reference type.
/// </summary>
public class RedisValueContainer
{
    public RedisValue Value { get; }
    public bool HasValue { get; set; }

    public RedisValueContainer(RedisValue value) => Value = value;
}

/// <summary>
/// A generic wrapper for a single Redis value result.
/// </summary>
public class RedisValueWrapper : NpOnWrapperResult<RedisValueContainer, IReadOnlyDictionary<string, INpOnCell>>, INpOnRowWrapper
{
    public RedisValueWrapper(RedisValueContainer parent) : base(parent)
    {
        if (!parent.HasValue)
        {
            SetFail(EDbError.RedisValueIsNull);
        }
        else
        {
            SetSuccess();
        }
    }

    protected override IReadOnlyDictionary<string, INpOnCell> CreateResult()
    {
        // Treat a single value as a row with one column named "value"
        var cell = new NpOnCell<string?>(Parent.Value.ToString(), System.Data.DbType.String, "redis:string");
        return new Dictionary<string, INpOnCell> { { "value", cell } };
    }

    public T? As<T>()
    {
        if (!Parent.Value.HasValue) return default;
        // A simple conversion for basic types, assuming JSON for complex types.
        // This can be expanded based on RedisUtils or other helpers.
        return (T)Convert.ChangeType(Parent.Value, typeof(T));
    }

    public IReadOnlyDictionary<string, INpOnCell> GetRowWrapper()
    {
        return Result;
    }
}

/// <summary>
/// Wraps the result of a Redis HGETALL command (a collection of hash entries).
/// </summary>
public class RedisHashWrapper : NpOnWrapperResult<HashEntry[], IReadOnlyDictionary<string, INpOnCell>>, INpOnTableWrapper, INpOnRowWrapper
{
    public RedisHashWrapper(HashEntry[]? parent) : base(parent)
    {
        if (parent == null)
        {
            SetFail(EDbError.RedisValueIsNull);
            return;
        }
        SetSuccess();
    }

    protected override IReadOnlyDictionary<string, INpOnCell> CreateResult()
    {
        return Parent.ToDictionary(
            entry => entry.Name.ToString(),
            entry => (INpOnCell)new NpOnCell<string?>(entry.Value.ToString(), System.Data.DbType.String, "redis:string")
        );
    }

    public IReadOnlyDictionary<int, INpOnRowWrapper?> RowWrappers => new Dictionary<int, INpOnRowWrapper?> { { 0, this } };

    public INpOnCollectionWrapper CollectionWrappers => new RedisHashFieldCollection(Parent);

    /// <summary>
    /// Implements INpOnRowWrapper to allow the hash to be treated as a single row.
    /// </summary>
    public IReadOnlyDictionary<string, INpOnCell> GetRowWrapper()
    {
        return Result;
    }
}

/// <summary>
/// Represents the collection of fields within a Redis Hash, allowing access by field name.
/// This mimics the column collection in a database table.
/// </summary>
public class RedisHashFieldCollection : IReadOnlyDictionary<string, INpOnCell>, INpOnCollectionWrapper
{
    private readonly IReadOnlyDictionary<string, INpOnCell> _fields;

    public RedisHashFieldCollection(HashEntry[]? hashEntries)
    {
        if (hashEntries == null)
        {
            _fields = new Dictionary<string, INpOnCell>();
            return;
        }

        _fields = hashEntries.ToDictionary(
            entry => entry.Name.ToString(),
            entry => (INpOnCell)new NpOnCell<string?>(entry.Value.ToString(), System.Data.DbType.String, "redis:string")
        );
    }

    // IReadOnlyDictionary implementation
    public INpOnCell this[string key] => _fields[key];
    public IEnumerable<string> Keys => _fields.Keys;
    public IEnumerable<INpOnCell> Values => _fields.Values;
    public int Count => _fields.Count;
    public bool ContainsKey(string key) => _fields.ContainsKey(key);
    public bool TryGetValue(string key, out INpOnCell value) => _fields.TryGetValue(key, out value);
    public IEnumerator<KeyValuePair<string, INpOnCell>> GetEnumerator() => _fields.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // INpOnCollectionWrapper implementation
    public IReadOnlyDictionary<int, INpOnColumnWrapper?> GetColumnWrapperByIndexes(int[] indexes)
    {
        // Not applicable for Redis Hash, which is key-based, not index-based.
        return new Dictionary<int, INpOnColumnWrapper?>(0);
    }

    public IReadOnlyDictionary<string, INpOnColumnWrapper?> GetColumnWrapperByColumnNames(string[]? columnNames = null)
    {
        // Not applicable for Redis Hash.
        return new Dictionary<string, INpOnColumnWrapper?>(0);
    }
}