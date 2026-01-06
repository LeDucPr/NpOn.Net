using System.Text;

namespace Common.Extensions.NpOn.CommonWebApplication.Middlewares;

public class MiddlewareLogger(RequestDelegate next, ILogger<MiddlewareLogger> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // ===== Log Request =====
        context.Request.EnableBuffering(); // Cho phép đọc nhiều lần
        string requestBody;

        using (var reader = new StreamReader(
                   context.Request.Body,
                   encoding: Encoding.UTF8,
                   detectEncodingFromByteOrderMarks: false,
                   bufferSize: 1024,
                   leaveOpen: true))
        {
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // Reset stream về đầu
        }

        logger.LogInformation("➡MiddlewareLogger Request {Method} {Url} \nBody: {Body}",
            context.Request.Method,
            context.Request.Path.Value,
            //string.Join("; ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}")),
            requestBody);

        // ===== Log Response =====
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await next(context); // Gọi middleware tiếp theo

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        logger.LogInformation("⬅MiddlewareLogger Response {StatusCode} \nBody: {Body}",
            context.Response.StatusCode,
            responseText);

        await responseBody.CopyToAsync(originalBodyStream);
    }
}

public static class LoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MiddlewareLogger>();
    }
}