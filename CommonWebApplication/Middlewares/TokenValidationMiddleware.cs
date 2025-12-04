using System.Net;
using System.Security.Claims;
using AccountServiceObject.BusinessObjects;
using CommonMode;
using CommonWebApplication.Services;
using Microsoft.Net.Http.Headers;
using ProjectEnums.AccountEnums;

namespace CommonWebApplication.Middlewares;

public class TokenValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ContextService contextService, AuthenService authenService)
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
                var identity = claimsPrincipal.Identity as ClaimsIdentity;
                var createdDateClaim = identity?.FindFirst(ContextService.TokenCreatedUtc)?.Value;
                var minuteExpireClaim = identity?.FindFirst($"{ContextService.MinuteExpirePrefix}")?.Value;

                if (string.IsNullOrEmpty(createdDateClaim) || string.IsNullOrEmpty(minuteExpireClaim))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Invalid or expired token.");
                    return; // stop pipeline
                }

                DateTime? tokenCreatedDate = createdDateClaim.FromIso8601ToDateTime(); // expired (time)
                if (tokenCreatedDate < DateTime.UtcNow)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Invalid or expired token.");
                    return; // stop pipeline
                }
                
                // check enabled token
                string? tokenSessionId = context.User.FindFirst(ContextService.SessionCode)?.Value;
                if (string.IsNullOrWhiteSpace(tokenSessionId))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Invalid or expired token.");
                    return; // stop pipeline
                }
                
                AccountLoginInfoObject? accountInfo =  await authenService.GetLogonInfoBySessionId(tokenSessionId);
                if (accountInfo == null || accountInfo.TokenStatus == ETokenStatus.Inactive)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Invalid or expired token.");
                    return; // stop pipeline
                }
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