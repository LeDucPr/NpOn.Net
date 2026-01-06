namespace Common.Extensions.NpOn.CommonMode;

public static class DateTimeMode
{
    /// <summary>
    /// Chuyển đổi một đối tượng DateTime sang chuỗi định dạng ISO 8601.
    /// </summary>
    /// <param name="dateTime">Đối tượng DateTime cần chuyển đổi.</param>
    /// <returns>Chuỗi biểu diễn DateTime theo chuẩn ISO 8601.</returns>
    public static string ToIso8601(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    /// <summary>
    /// Chuyển đổi một đối tượng DateTimeOffset sang chuỗi định dạng ISO 8601.
    /// </summary>
    /// <param name="dateTimeOffset">Đối tượng DateTimeOffset cần chuyển đổi.</param>
    /// <returns>Chuỗi biểu diễn DateTimeOffset theo chuẩn ISO 8601.</returns>
    public static string ToIso8601(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:sszzz");
    }


    
    /// <summary>
    /// Chuyển đổi một chuỗi định dạng ISO 8601 sang đối tượng DateTime.
    /// </summary>
    /// <param name="iso8601String">Chuỗi ISO 8601 cần chuyển đổi.</param>
    /// <returns>Đối tượng DateTime tương ứng.</returns>
    public static DateTime FromIso8601ToDateTime(this string iso8601String)
    {
        return DateTime.Parse(iso8601String);
    }

    /// <summary>
    /// Chuyển đổi một chuỗi định dạng ISO 8601 sang đối tượng DateTimeOffset.
    /// </summary>
    /// <param name="iso8601String">Chuỗi ISO 8601 cần chuyển đổi.</param>
    /// <returns>Đối tượng DateTimeOffset tương ứng.</returns>
    public static DateTimeOffset FromIso8601ToDateTimeOffset(this string iso8601String)
    {
        return DateTimeOffset.Parse(iso8601String);
    }


    
    /// <summary>
    /// Chuyển đổi một đối tượng DateTime sang chuỗi với định dạng tùy chỉnh.
    /// </summary>
    /// <param name="dateTime">Đối tượng DateTime cần chuyển đổi.</param>
    /// <param name="format">Chuỗi định dạng tùy chỉnh.</param>
    /// <returns>Chuỗi biểu diễn DateTime theo định dạng đã cho.</returns>
    public static string ToString(this DateTime dateTime, string format)
    {
        return dateTime.ToString(format);
    }

    /// <summary>
    /// Chuyển đổi một chuỗi sang đối tượng DateTime với định dạng tùy chỉnh.
    /// </summary>
    /// <param name="dateTimeString">Chuỗi cần chuyển đổi.</param>
    /// <param name="format">Chuỗi định dạng tùy chỉnh.</param>
    /// <returns>Đối tượng DateTime tương ứng.</returns>
    public static DateTime ToDateTime(this string dateTimeString, string format)
    {
        return DateTime.ParseExact(dateTimeString, format, null);
    }
}