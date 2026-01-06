using System.Globalization;
using System.Text;

namespace Common.Extensions.NpOn.CommonMode;

public static class DefaultValueMode
{
    public const int DefaultValueForEnumInt = 0;
    public const int DefaultValueForInt = 0;
    public const long DefaultValueForLong = 0;
}

public static class DefaultValueModeExtensions
{
    public static int EnumAsInt<TEnum>(this TEnum? value) where TEnum : struct, Enum
        => Convert.ToInt32(value);

    public static int EnumAsInt<TEnum>(this TEnum value) where TEnum : struct, Enum
        => Convert.ToInt32(value);

    public static long EnumAsLong<TEnum>(this TEnum value) where TEnum : struct, Enum
        => Convert.ToInt64(value);

    public static long EnumAsLong<TEnum>(this TEnum? value) where TEnum : struct, Enum
        => Convert.ToInt64(value);

    public static byte EnumAsByte<TEnum>(this TEnum? value) where TEnum : struct, Enum
        => Convert.ToByte(value);

    public static byte EnumAsByte<TEnum>(this TEnum value) where TEnum : struct, Enum
        => Convert.ToByte(value);

    public static int AsDefaultInt(this object? obj)
    {
        if (obj == null)
            return DefaultValueMode.DefaultValueForInt;
        if (int.TryParse(obj.ToString(), out int result))
            return result;
        return DefaultValueMode.DefaultValueForInt;
    }


    public static long AsDefaultLong(this object? obj)
    {
        if (obj == null)
            return DefaultValueMode.DefaultValueForLong;
        if (long.TryParse(obj.ToString(), out long result))
            return result;
        return DefaultValueMode.DefaultValueForLong;
    }


    public static int AsDefaultEnum<TEnum>(this object? obj) where TEnum : struct, Enum
    {
        if (obj == null)
            return DefaultValueMode.DefaultValueForEnumInt;
        if (Enum.TryParse(obj.ToString(), out TEnum result))
            return (int)(object)result;
        return DefaultValueMode.DefaultValueForEnumInt;
    }


    public static Guid AsDefaultGuid(this object? obj)
    {
        if (obj == null)
            return Guid.Empty;

        if (obj is Guid guid)
            return guid;

        if (Guid.TryParse(obj.ToString()?.Trim(), out var result))
            return result;

        return Guid.Empty;
    }
    public static string AsDefaultAscii(this object? obj)
    {
        if (obj == null)
            return string.Empty;

        var input = obj.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        var result = sb.ToString().Normalize(NormalizationForm.FormC);
        // tiếng Việt
        result = result.Replace('Đ', 'D').Replace('đ', 'd');

        return result;
    }



    public static string AsDefaultString(this object? obj)
    {
        if (obj == null)
            return string.Empty;
        return obj.ToString() ?? string.Empty;
    }


    public static string AsEmptyString(this object? obj)
    {
        if (obj == null)
            return string.Empty;
        return obj.ToString()?.Trim() ?? string.Empty;
    }


    public static DateTime AsDefaultDateTime(this object? obj)
    {
        if (obj == null)
            return DateTime.MinValue;
        if (DateTime.TryParse(obj.ToString(), out DateTime result))
            return result;
        return DateTime.MinValue;
    }


    public static bool AsDefaultBool(this object? obj)
    {
        if (obj == null)
            return false;
        if (bool.TryParse(obj.ToString(), out bool result))
            return result;
        return false;
    }


    // convert to world standard
    public static DateTime AsDefaultStandardDateTime(this object? obj)
    {
        if (obj == null)
            return DateTime.MinValue;
        if (DateTime.TryParse(obj.ToString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal,
                out DateTime result)
           )
            return result;
        return DateTime.MinValue;
    }

    public static DateTime AsUtcDateTime(this object? obj)
    {
        if (obj == null)
            return DateTime.MinValue;
        if (DateTime.TryParse(obj.ToString(), out DateTime result))
        {
            if (result.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(result, DateTimeKind.Utc);
            return result.ToUniversalTime();
        }

        return DateTime.MinValue;
    }

    public static string AsArrayJoin(this IEnumerable<string>? strings)
        => strings != null ? string.Join(",", strings) : string.Empty;

    public static string AsArrayJoin(this IEnumerable<string>? strings, string separator)
        => strings != null ? string.Join(separator, strings) : string.Empty;

    // int
    public static string AsArrayJoin(this IEnumerable<int>? ints)
        => ints != null ? string.Join(",", ints) : string.Empty;

    public static string AsArrayJoin(this IEnumerable<int>? ints, string separator)
        => ints != null ? string.Join(separator, ints) : string.Empty;

    // long
    public static string AsArrayJoin(this IEnumerable<long>? longs)
        => longs != null ? string.Join(",", longs) : string.Empty;

    public static string AsArrayJoin(this IEnumerable<long>? longs, string separator)
        => longs != null ? string.Join(separator, longs) : string.Empty;

    // short
    public static string AsArrayJoin(this IEnumerable<short>? shorts)
        => shorts != null ? string.Join(",", shorts) : string.Empty;

    public static string AsArrayJoin(this IEnumerable<short>? shorts, string separator)
        => shorts != null ? string.Join(separator, shorts) : string.Empty;

    // byte
    public static string AsArrayJoin(this IEnumerable<byte>? bytes)
        => bytes != null ? string.Join(",", bytes) : string.Empty;

    public static string AsArrayJoin(this IEnumerable<byte>? bytes, string separator)
        => bytes != null ? string.Join(separator, bytes) : string.Empty;

    // bool
    public static string AsArrayJoin(this IEnumerable<bool>? bools)
        => bools != null ? string.Join(",", bools) : string.Empty;

    public static string AsArrayJoin(this IEnumerable<bool>? bools, string separator)
        => bools != null ? string.Join(separator, bools) : string.Empty;

    // float
    public static string AsArrayJoin(this IEnumerable<float>? floats)
        => floats != null ? string.Join(",", floats) : string.Empty;

    public static string AsArrayJoin(this IEnumerable<float>? floats, string separator)
        => floats != null ? string.Join(separator, floats) : string.Empty;

    // double
    public static string AsArrayJoin(this IEnumerable<double>? doubles)
        => doubles != null ? string.Join(",", doubles) : string.Empty;

    public static string AsArrayJoin(this IEnumerable<double>? doubles, string separator)
        => doubles != null ? string.Join(separator, doubles) : string.Empty;

    // decimal
    public static string AsArrayJoin(this IEnumerable<decimal>? decimals)
        => decimals != null ? string.Join(",", decimals) : string.Empty;

    public static string AsArrayJoin(this IEnumerable<decimal>? decimals, string separator)
        => decimals != null ? string.Join(separator, decimals) : string.Empty;

    // char
    public static string AsArrayJoin(this IEnumerable<char>? chars)
        => chars != null ? string.Join(",", chars) : string.Empty;

    public static string AsArrayJoin(this IEnumerable<char>? chars, string separator)
        => chars != null ? string.Join(separator, chars) : string.Empty;

    // Guid
    public static string AsArrayJoin(this IEnumerable<Guid>? guids)
        => guids != null ? string.Join(",", guids) : string.Empty;

    public static string AsArrayJoin(this IEnumerable<Guid>? guids, string separator)
        => guids != null ? string.Join(separator, guids) : string.Empty;

    // object
    public static string AsArrayJoin(this IEnumerable<object>? objs)
        => objs != null ? string.Join(",", objs.Select(x => x.ToString())) : string.Empty;

    public static string AsArrayJoin(this IEnumerable<object>? objs, string separator)
        => objs != null ? string.Join(separator, objs.Select(x => x.ToString())) : string.Empty;
}