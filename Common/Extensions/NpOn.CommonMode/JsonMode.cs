using Newtonsoft.Json;

namespace Common.Extensions.NpOn.CommonMode;

public static class JsonMode
{
    public static string ToJson(object? obj) => JsonConvert.SerializeObject(obj);
    public static T? FromJson<T>(string? json) => JsonConvert.DeserializeObject<T>(json ?? string.Empty);

    public static object? FromJson(string? json, Type type) =>
        JsonConvert.DeserializeObject(json ?? string.Empty, type);
    
    public static bool TryFromJson<T>(string? json, out T? result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(json))
            return false;
        try
        {
            result = JsonConvert.DeserializeObject<T>(json);
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryFromJson(string? json, Type type, out object? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(json))
            return false;
        try
        {
            result = JsonConvert.DeserializeObject(json, type);
            return result != null;
        }
        catch
        {
            return false;
        }
    }
}