using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CommonMode;

public static class EnumMode
{
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