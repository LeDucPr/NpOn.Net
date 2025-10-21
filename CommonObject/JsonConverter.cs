using Newtonsoft.Json;

namespace CommonObject;

public static class JsonConverter
{
    public static string ToJson(object? obj) => JsonConvert.SerializeObject(obj);
    public static T? FromJson<T>(string? json) => JsonConvert.DeserializeObject<T>(json ?? string.Empty);
}