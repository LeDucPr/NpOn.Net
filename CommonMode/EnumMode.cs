using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Enums;
using Microsoft.Extensions.Configuration;

namespace CommonMode;

public static class EnumMode
{
    #region ConfigMode

    public static void InitGlobal(this IConfiguration configuration)
    {
        var keys = Enum.GetValues(typeof(EApplicationConfiguration));
        foreach (EApplicationConfiguration key in keys)
        {
            if (Configs.ContainsKey(key)) continue;
            string keyConfig = key.GetDisplayName();
            var config = configuration.GetSection(keyConfig);
            string? valueConfig = config.Value;
            Configs.Add(key, valueConfig ?? string.Empty);
        }
    }

    private static readonly IDictionary<EApplicationConfiguration, string> Configs =
        new Dictionary<EApplicationConfiguration, string>();

    public static string? GetAppSettingConfig(this EApplicationConfiguration eApplication)
    {
        Configs.TryGetValue(eApplication, out var value);
        return value;
    }

    #endregion ConfigMode


    #region Flag Operations

    public static bool HasFlag<TEnum>(this TEnum value, TEnum flag) where TEnum : struct, Enum
    {
        return value.HasFlag(flag);
    }

    public static bool HasAllFlags<TEnum>(this TEnum value, params TEnum[]? flags) where TEnum : struct, Enum
    {
        if (flags == null || flags.Length == 0) // always exist flag (0)
            return true;
        return flags.All(@enum => value.HasFlag(@enum));
    }

    public static bool HasAnyFlag<TEnum>(this TEnum value, params TEnum[]? flags) where TEnum : struct, Enum
    {
        if (flags == null || flags.Length == 0)
            return false;
        return flags.Any(@enum => value.HasFlag(@enum));
    }

    public static TEnum[] GetFlags<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        if (Convert.ToInt64(value) == 0)
            return [];
        return Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Where(e => Convert.ToInt64(e) != 0 && value.HasFlag(e))
            .ToArray();
    }

    public static TEnum[] GetAllInitEnum<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Where(e => Convert.ToInt64(e) != 0)
            .ToArray();
    }

    #endregion


    #region Get Name from Enum

    public static string GetDisplayName<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
    {
        var fieldInfo = typeof(TEnum).GetField(enumValue.ToString());
        if (fieldInfo == null) return enumValue.ToString();

        var displayAttribute = fieldInfo.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.Name ?? enumValue.ToString();
    }

    public static string GetShortName<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
    {
        var fieldInfo = typeof(TEnum).GetField(enumValue.ToString());
        if (fieldInfo == null) return enumValue.ToString();

        var displayAttribute = fieldInfo.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.ShortName ?? enumValue.ToString();
    }

    /// <summary>
    /// Chuyển đôi chỉ sử dụng với tên của Enum và giá trị của Enum
    /// </summary>
    /// <param name="enumString"></param>
    /// <typeparam name="TEnum"></typeparam>
    /// <returns></returns>
    public static TEnum? ToEnum<TEnum>(this string enumString) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(enumString))
            return null;
        string memberName = enumString;
        int lastDotIndex = enumString.LastIndexOf('.');
        if (lastDotIndex >= 0)
            memberName = enumString.Substring(lastDotIndex + 1);
        if (Enum.TryParse(memberName, true, out TEnum result))
            return result;
        return null;
    }
    
    public static TEnum? IntAsStringToEnum<TEnum>(this string enumString) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(enumString))
            return null;
        if (int.TryParse(enumString, out int intValue))
            if (Enum.IsDefined(typeof(TEnum), intValue))
                return (TEnum)(object)intValue;
        return null;
    }
    
    public static TEnum? ToEnum<TEnum>(this long enumValue) where TEnum : struct, Enum
        => Enum.IsDefined(typeof(TEnum), enumValue) ? (TEnum)(object)enumValue : null;

    public static TEnum? ToEnum<TEnum>(this int enumValue) where TEnum : struct, Enum
        => Enum.IsDefined(typeof(TEnum), enumValue) ? (TEnum)(object)enumValue : null;

    public static TEnum? ToEnum<TEnum>(this byte enumValue) where TEnum : struct, Enum
        => Enum.IsDefined(typeof(TEnum), enumValue) ? (TEnum)(object)enumValue : null;

    #endregion


    #region Get Enum from Name

    public static TEnum GetEnumValueFromDisplayName<TEnum>(string displayName, bool ignoreCase = false)
        where TEnum : struct, Enum
    {
        return GetValueFromDisplayAttribute<TEnum>(displayName, attr => attr.Name, ignoreCase);
    }

    public static TEnum GetEnumValueFromShortName<TEnum>(string shortName, bool ignoreCase = false)
        where TEnum : struct, Enum
    {
        return GetValueFromDisplayAttribute<TEnum>(shortName, attr => attr.ShortName, ignoreCase);
    }

    #endregion


    #region Generic Attribute Getter

    public static TAttribute? GetAttribute<TEnum, TAttribute>(this TEnum enumValue)
        where TEnum : struct, Enum
        where TAttribute : Attribute
    {
        var fieldInfo = typeof(TEnum).GetField(enumValue.ToString());
        return fieldInfo?.GetCustomAttribute<TAttribute>();
    }

    #endregion


    #region Private Helpers

    private static TEnum GetValueFromDisplayAttribute<TEnum>(string valueToFind,
        Func<DisplayAttribute, string?> propertySelector, bool ignoreCase) where TEnum : struct, Enum
    {
        var stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        var enumType = typeof(TEnum);
        foreach (var enumValue in Enum.GetValues(enumType).Cast<TEnum>())
        {
            var fieldInfo = enumType.GetField(enumValue.ToString());
            if (fieldInfo == null) continue;

            var displayAttribute = fieldInfo.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null)
            {
                string? propertyValue = propertySelector(displayAttribute);
                if (string.Equals(propertyValue, valueToFind, stringComparison))
                {
                    return enumValue;
                }
            }
        }

        foreach (var enumName in Enum.GetNames(enumType))
        {
            if (string.Equals(enumName, valueToFind, stringComparison))
            {
                return (TEnum)Enum.Parse(enumType, enumName, ignoreCase);
            }
        }

        throw new ArgumentException(
            $"No enum value of type '{enumType.Name}' found for the display value '{valueToFind}'.",
            nameof(valueToFind));
    }

    #endregion
}