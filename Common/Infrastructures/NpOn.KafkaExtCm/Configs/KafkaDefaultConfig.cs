using System.Collections.ObjectModel;
using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;

namespace Common.Infrastructures.NpOn.KafkaExtCm.Configs;

public static class KafkaDefaultConfig
{
    public static IReadOnlyDictionary<EKafkaTopicConfig, (string Key, string DefaultValue)> TopicConfigs { get; }

    public static IReadOnlyDictionary<EKafkaConsumerConfig, (string Key, string DefaultValue)> ConsumerConfigs { get; }

    static KafkaDefaultConfig()
    {
        TopicConfigs = LoadConfig<EKafkaTopicConfig>();
        ConsumerConfigs = LoadConfig<EKafkaConsumerConfig>();
    }

    private static IReadOnlyDictionary<TEnum, (string Key, string DefaultValue)> LoadConfig<TEnum>()
        where TEnum : struct, Enum
    {
        var dict = new Dictionary<TEnum, (string Key, string DefaultValue)>();
        foreach (var enumValue in EnumMode.GetAllInitEnum<TEnum>())
        {
            dict[enumValue] = (enumValue.GetDisplayName(), enumValue.GetDisplayDescription());
        }

        return new ReadOnlyDictionary<TEnum, (string Key, string DefaultValue)>(dict);
    }

    public static Dictionary<string, string> GetAllConfigAsString<TEnum>(
        this IReadOnlyDictionary<TEnum, (string Key, string DefaultValue)> config)
        where TEnum : struct, Enum
    {
        return new Dictionary<string, string>(config.ToDictionary(x => x.Value.Key, x => x.Value.DefaultValue));
    }

    public static void Set<TEnum>(this IDictionary<TEnum, (string Key, string DefaultValue)> dictionary, TEnum key,
        object value)
        where TEnum : struct, Enum
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = (dictionary[key].Key, value.ToString()!);
            return;
        }

        dictionary.Add(key, (dictionary[key].Key, value.ToString()!));
    }
}