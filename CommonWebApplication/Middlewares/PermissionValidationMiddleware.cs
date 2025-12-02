using System.Security.Claims;
using CommonMode;
using CommonObject;
using CommonWebApplication.Attributes;
using CommonWebApplication.Services;
using ProjectEnums.AccountEnums;
using Microsoft.AspNetCore.Authorization;

namespace CommonWebApplication.Middlewares;

public class PermissionValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ContextService contextService)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            return;
        }

        // AllowAnonymous
        var isAnonymousAllowed = endpoint.Metadata.Any(m => m.GetType() == typeof(AllowAnonymousAttribute));
        if (isAnonymousAllowed)
        {
            await next(context);
            return;
        }

        PermissionControllerAttribute? permissionControllerAttribute = endpoint.Metadata
            .OfType<PermissionControllerAttribute>()
            .FirstOrDefault();

        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        bool isHasPermission = false;
        if (permissionControllerAttribute != null)
        {
            var claimsPrincipal = context.User;
            var identity = claimsPrincipal.Identity as ClaimsIdentity;
            EPermission permission = identity?.FindFirst(ContextService.Permission)?.Value.ToEnum<EPermission>() ??
                                     EPermission.Unknown;
            isHasPermission = permissionControllerAttribute.IsHasPermission(permission);
        }

        // Không có PermissionControllerAttribute => pass
        if (isHasPermission)
        {
            // !! Nếu có Global Policy yêu cầu mọi API phải có quyền
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
}

public static class PermissionValidationMiddlewareExtensions
{
    public static IApplicationBuilder UsePermissionValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PermissionValidationMiddleware>();
    }
}