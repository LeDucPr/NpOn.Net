using System.Collections.Concurrent;

namespace CommonObject;

public class WrapperCacheStore<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, WrapperCache<TValue>> _dict = new();

    public Dictionary<TKey, TValue> GetAll()
    {
        return _dict.Where(x => !x.Value.IsExpired).ToDictionary(x => x.Key, x => x.Value.Value);
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory, TimeSpan? expiresIn = null)
    {
        if (_dict.TryGetValue(key, out var existing))
        {
            if (!existing.IsExpired)
                return existing.Value;

            _dict.TryRemove(key, out _);
        }

        var newValue = factory(key);
        var wrapper = new WrapperCache<TValue>(newValue, expiresIn);
        return _dict.GetOrAdd(key, wrapper).Value;
    }

    public TValue AddOrUpdate(TKey key, TValue value, TimeSpan? expiresIn = null)
    {
        var wrapper = new WrapperCache<TValue>(value, expiresIn);
        _dict.AddOrUpdate(key, wrapper, (_, _) => wrapper);
        return value;
    }

    public TValue AddOrUpdate(
        TKey key,
        Func<TKey, TValue> addFactory,
        Func<TKey, TValue, TValue> updateFactory,
        TimeSpan? expiresIn = null)
    {
        return _dict.AddOrUpdate(
            key,
            k => new WrapperCache<TValue>(addFactory(k), expiresIn),
            (k, existing) =>
            {
                if (!existing.IsExpired)
                {
                    var updated = updateFactory(k, existing.Value);
                    return new WrapperCache<TValue>(updated, expiresIn);
                }
                else
                {
                    // Nếu hết hạn thì coi như add mới
                    return new WrapperCache<TValue>(addFactory(k), expiresIn);
                }
            }
        ).Value;
    }

    public bool TryGetValue(TKey key, out TValue? value)
    {
        value = default!;
        if (_dict.TryGetValue(key, out var wrapper))
        {
            if (!wrapper.IsExpired)
            {
                value = wrapper.Value;
                return true;
            }

            _dict.TryRemove(key, out _);
        }

        return false;
    }

    public bool Remove(TKey key) => _dict.TryRemove(key, out _);
}