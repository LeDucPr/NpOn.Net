namespace CommonObject;

public static class DefaultValueForObject
{
    public const int DefaultValueForEnumInt = 0;
    public const int DefaultValueForInt = 0;
    public const long DefaultValueForLong = 0;
}

public static class DefaultValueForObjectExtensions
{
    public static int AsDefaultInt(this object? obj)
    {
        if (obj == null)
            return DefaultValueForObject.DefaultValueForInt;
        if (int.TryParse(obj.ToString(), out int result))
            return result;
        return DefaultValueForObject.DefaultValueForInt;
    }

    public static long AsDefaultLong(this object? obj)
    {
        if (obj == null)
            return DefaultValueForObject.DefaultValueForLong;
        if (long.TryParse(obj.ToString(), out long result))
            return result;
        return DefaultValueForObject.DefaultValueForLong;
    }

    public static int AsDefaultEnum<TEnum>(this object? obj) where TEnum : struct, Enum
    {
        if (obj == null)
            return DefaultValueForObject.DefaultValueForEnumInt;
        if (Enum.TryParse(obj.ToString(), out TEnum result))
            return (int)(object)result;
        return DefaultValueForObject.DefaultValueForEnumInt;
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

    // chuyển sang định dạng ngày tiêu chuẩn của thế giới 
    public static DateTime AsDefaultStandardDateTime(this object? obj)
    {
        if (obj == null)
            return DateTime.MinValue;
        if (DateTime.TryParse(obj.ToString(),
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AdjustToUniversal,
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
}