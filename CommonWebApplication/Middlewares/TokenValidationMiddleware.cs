using System.Net;
using CommonWebApplication.Services;
using Microsoft.Net.Http.Headers;

namespace CommonWebApplication.Middlewares;

public class TokenValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ContextService contextService)
    {
        string? authorizationHeader = context.Request.Headers[HeaderNames.Authorization];
        // Only use Authorization header for Bearer token
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
        {
            var token = authorizationHeader["Bearer ".Length..].Trim();
            if (contextService.ValidateToken(token, out var claimsPrincipal))
            {
                if (claimsPrincipal == null)
                    return;
                context.User = claimsPrincipal;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Invalid or expired token.");
                return; // stop pipeline
            }
            
        }

        await next(context);
    }
}

public static class TokenValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenValidationMiddleware>();
    }
}