using System.Net;
using System.Security.Claims;
using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;

namespace Common.Extensions.NpOn.CommonWebApplication.Middlewares
{
    public class TokenValidationMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, ContextService contextService, AuthenService authenService)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            {
                await next(context);
                return;
            }

            PathString path = context.Request.Path;
            bool isMediaEnablePublicDownload =
                EApplicationConfiguration.IsMediaEnablePublicDownload.GetAppSettingConfig().AsDefaultBool();
            if (isMediaEnablePublicDownload)
            {
                string downloadPath =
                    EApplicationConfiguration.MediaDownloadUrlPrefix.GetAppSettingConfig().AsEmptyString();
                if (path.StartsWithSegments(downloadPath))
                {
                    await next(context);
                    return;
                }
            }

            string? authorizationHeader = context.Request.Headers[HeaderNames.Authorization];
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

                    AccountLoginRModel? accountInfo = await authenService.GetLogonInfoBySessionId(tokenSessionId);
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
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Authorization header is missing or malformed.");
                return; // stop pipeline
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
}