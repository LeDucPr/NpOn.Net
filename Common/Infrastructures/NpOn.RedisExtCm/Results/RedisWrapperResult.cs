using System.Collections;
using Common.Extensions.NpOn.CommonEnums;
using Common.Infrastructures.NpOn.CommonDb.DbResults;
using StackExchange.Redis;

namespace Common.Infrastructures.NpOn.RedisExtCm.Results;

/// <summary>
/// A container class to wrap the RedisValue struct, allowing it to be used as a reference type.
/// </summary>
public class RedisValueContainer
{
    public RedisValue SingleValue { get; }
    public RedisValue[]? Values { get; }
    public bool HasValue => IsSingle ? SingleValue.HasValue : (Values?.Length > 0);
    public bool IsSingle { get; }

    public RedisValueContainer(RedisValue value)
    {
        SingleValue = value;
        IsSingle = true;
    }

    public RedisValueContainer(RedisValue[] values)
    {
        Values = values;
        IsSingle = false;
    }
}

/// <summary>
/// A generic wrapper for Redis value results. Can act as a single row or a table depending on the content.
/// </summary>
public class RedisValueWrapper : NpOnWrapperResult<RedisValueContainer, IReadOnlyDictionary<string, INpOnCell>>, INpOnRowWrapper, INpOnTableWrapper
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

    /// <summary>
    /// Creates the result dictionary. For a single value, it's a one-cell dictionary. For multiple values, it's the first value.
    /// </summary>
    protected override IReadOnlyDictionary<string, INpOnCell> CreateResult()
    {
        var valueToUse = Parent.IsSingle ? Parent.SingleValue : (Parent.Values?.FirstOrDefault() ?? RedisValue.Null);
        var cell = new NpOnCell<string?>(valueToUse.ToString(), System.Data.DbType.String, "redis:string");
        return new Dictionary<string, INpOnCell> { { "value", cell } };
    }

    /// <summary>
    /// Gets the first value as the specified type.
    /// </summary>
    public T? As<T>()
    {
        var valueToUse = Parent.IsSingle ? Parent.SingleValue : (Parent.Values?.FirstOrDefault() ?? RedisValue.Null);
        if (!valueToUse.HasValue) return default;

        // A simple conversion for basic types, assuming JSON for complex types.
        // This can be expanded based on RedisUtils or other helpers.
        return (T)Convert.ChangeType(valueToUse, typeof(T));
    }

    /// <summary>
    /// Gets all values as an array of the specified type.
    /// </summary>
    public T?[]? AsArray<T>()
    {
        if (Parent.IsSingle)
        {
            return Parent.SingleValue.HasValue ? new[] { As<T>() } : Array.Empty<T?>();
        }

        return Parent.Values?.Select(v =>
        {
            if (!v.HasValue) return default;
            return (T)Convert.ChangeType(v, typeof(T));
        }).ToArray();
    }

    // INpOnRowWrapper implementation (returns the first value/row)
    public IReadOnlyDictionary<string, INpOnCell> GetRowWrapper()
    {
        return Result;
    }

    // INpOnTableWrapper implementation (for multiple values)
    public IReadOnlyDictionary<int, INpOnRowWrapper?> RowWrappers =>
        Parent.Values?.Select((value, index) => new { value, index })
            .ToDictionary(
                item => item.index,
                item => (INpOnRowWrapper?)new RedisValueWrapper(new RedisValueContainer(item.value))
            ) ?? new Dictionary<int, INpOnRowWrapper?>();

    public INpOnCollectionWrapper CollectionWrappers => throw new NotImplementedException("Collection wrapper is not supported for a simple list of Redis values.");
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