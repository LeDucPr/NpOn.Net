using System.IdentityModel.Tokens.Jwt;
using System.Net;
using AccountServiceObject.BusinessObjects;
using CommonWebApplication.Services;
using IAccountService;
using Microsoft.AspNetCore.Authorization;

namespace SSO.Middlewares;

public class AuthenFilterHandlerMiddleware(
    ContextService contextService,
    IAuthenticationService authenticationService,
    IUserService userService,
    ILogger<AuthenFilterHandlerMiddleware> logger,
    AuthenService authenService
) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {            
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                await next(context);
                return;
            }
            
            // Logic xác thực chỉ chạy cho các endpoint cần bảo vệ
            var userLogin = contextService.UserInfo();
            if (userLogin == null)
            {
                var tokenClaimType = context.User.Claims.FirstOrDefault(c => c.Type == JwtHeaderParameterNames.Typ);
                if (tokenClaimType != null)
                {
                    var sessionId = context.User.Claims.FirstOrDefault(c => c.Type == ContextService.SessionCode);
                    var userResponse = await userService.GetAccountInfo();
                    if (userResponse.Data == null)
                    {
                        await WriteErrorResponse(context, HttpStatusCode.Unauthorized);
                        throw new InvalidDataException("Invalid user");
                    }
                    // await authenService.SetLoginInfo(userInfo.SessionId, userInfo,
                    //     userInfo.MinuteExpire);
                    // userLogin = userInfo;
                }
            }
            await next(context);
        }
        catch (InvalidDataException ex)
        {
            await WriteErrorResponse(context, HttpStatusCode.Unauthorized, ex.Message + ex.InnerException);
        }
        catch (Exception ex)
        {
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError, ex.Message + ex.InnerException);
        }
    }

    private async Task WriteErrorResponse(HttpContext context,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string? message = null,
        string? sessionId = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        if (!string.IsNullOrEmpty(message))
            logger.LogError("{Message}", message);
    }
}

public static class LoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthenFilterHandlerMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenFilterHandlerMiddleware>();
    }
}