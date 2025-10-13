using System.Collections.Concurrent;

namespace CommonObject;

public class WrapperCache<TValue>
{
    public TValue Value { get; }
    private DateTime Expiration { get; }

    private const int ExpiresInSecond = 60;

    public WrapperCache(TValue value, TimeSpan? expiresIn = null)
    {
        Value = value;
        Expiration = DateTime.UtcNow + (expiresIn ?? TimeSpan.FromSeconds(ExpiresInSecond));
    }

    public bool IsExpired => DateTime.UtcNow > Expiration;
}

public static class WrapperCacheExtensions
{
    public static TValue AddOrUpdate<TKey, TValue>(
        this ConcurrentDictionary<TKey, WrapperCache<TValue>> dict,
        TKey key,
        TValue value,
        TimeSpan? expiresIn = null) where TKey : notnull
    {
        var wrapper = new WrapperCache<TValue>(value, expiresIn);
        dict.AddOrUpdate(key, wrapper, (_, _) => wrapper);
        return value;
    }

    public static TValue GetOrAdd<TKey, TValue>(
        this ConcurrentDictionary<TKey, WrapperCache<TValue>> dict,
        TKey key,
        Func<TKey, TValue> valueFactory,
        TimeSpan? expiresIn = null) where TKey : notnull
    {
        if (dict.TryGetValue(key, out var existing))
        {
            if (!existing.IsExpired)
                return existing.Value;

            dict.TryRemove(key, out _); // remove nếu hết hạn
        }

        var newValue = valueFactory(key);
        var wrapper = new WrapperCache<TValue>(newValue, expiresIn);
        return dict.GetOrAdd(key, wrapper).Value;
    }

    public static bool TryGetValue<TKey, TValue>(
        this ConcurrentDictionary<TKey, WrapperCache<TValue>> dict,
        TKey key,
        out TValue value) where TKey : notnull
    {
        value = default!;
        if (dict.TryGetValue(key, out var wrapper))
        {
            if (!wrapper.IsExpired)
            {
                value = wrapper.Value;
                return true;
            }

            dict.TryRemove(key, out _);
        }

        return false;
    }

    public static void Remove<TKey, TValue>(
        this ConcurrentDictionary<TKey, WrapperCache<TValue>> dict,
        TKey key) where TKey : notnull
    {
        dict.TryRemove(key, out _);
    }
}