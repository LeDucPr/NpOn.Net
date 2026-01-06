using System.Text.Json;
using StackExchange.Redis;

namespace Common.Infrastructures.NpOn.RedisExtCm.Results;

/// <summary>
/// Provides utility and extension methods for working with Redis data types.
/// </summary>
public static class RedisUtils
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Converts a RedisValue to a specified type T.
    /// </summary>
    public static T? As<T>(this RedisValue value)
    {
        if (!value.HasValue)
        {
            return default;
        }

        var targetType = typeof(T);

        if (targetType == typeof(string)) return (T)(object)value.ToString();
        if (targetType == typeof(int)) return (T)(object)(int)value;
        if (targetType == typeof(long)) return (T)(object)(long)value;
        if (targetType == typeof(double)) return (T)(object)(double)value;
        if (targetType == typeof(bool)) return (T)(object)(bool)value;
        if (targetType == typeof(byte[])) return (T)(object)(byte[])value!;

        // For complex types, assume it's a JSON string
        try
        {
            return JsonSerializer.Deserialize<T>(value.ToString(), JsonOptions);
        }
        catch
        {
            return default;
        }
    }
}