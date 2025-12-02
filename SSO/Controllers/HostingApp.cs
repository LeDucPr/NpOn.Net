using CommonObject;
using CommonWebApplication.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ProjectEnums.AccountEnums;

namespace SSO.Controllers;

public class HostingApp : IHostedService
{
    private readonly ILogger<HostingApp> _logger;
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

    public HostingApp(
        ILogger<HostingApp> logger,
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
    {
        _logger = logger;
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // _logger.LogInformation("Starting to list all registered API endpoints...");
        //
        // var endpoints = _actionDescriptorCollectionProvider.ActionDescriptors.Items
        //     .Where(ad => ad.AttributeRouteInfo != null) // Chỉ lấy các endpoint có route attribute
        //     .ToList();
        //
        // if (!endpoints.Any())
        // {
        //     _logger.LogWarning("No API endpoints with attribute routes found.");
        //     return Task.CompletedTask;
        // }
        //
        // _logger.LogInformation("Discovered Endpoints:");
        // foreach (var endpoint in endpoints)
        // {
        //     // Lấy phương thức HTTP (GET, POST, etc.)
        //     var httpMethods = endpoint.EndpointMetadata
        //                               .OfType<HttpMethodMetadata>()
        //                               .SelectMany(m => m.HttpMethods)
        //                               .Distinct();
        //
        //     var methods = string.Join(", ", httpMethods);
        //     if (string.IsNullOrEmpty(methods))
        //     {
        //         methods = "N/A";
        //     }
        //
        //     // Lấy template của route
        //     var routeTemplate = endpoint.AttributeRouteInfo!.Template;
        //
        //     _logger.LogInformation("-> {Methods} /{Route}", methods, routeTemplate);
        // }

        var endpoints = _actionDescriptorCollectionProvider.ActionDescriptors.Items
            .Where(ad => ad.AttributeRouteInfo != null) // Chỉ lấy các endpoint có route attribute
            .ToList();
        foreach (var endpoint in endpoints)
        {
            // 1. Kiểm tra thuộc tính PermissionController (lấy từ Controller HOẶC Action)
            var permissionControllerAttribute = endpoint.EndpointMetadata
                .OfType<PermissionControllerAttribute>()
                .FirstOrDefault();

            EPermission requiredPermission = EPermission.Administrator;
            if (permissionControllerAttribute != null)
            {
                requiredPermission = permissionControllerAttribute.IsHasPermission(requiredPermission)
                    ? requiredPermission
                    : EPermission.Unknown;
            }

            // 2. Kiểm tra thuộc tính AllowAnonymous (thường được ưu tiên hơn quyền)
            var isAnonymousAllowed = endpoint.EndpointMetadata
                .Any(m => m.GetType() == typeof(AllowAnonymousAttribute));

            var permissionInfo = requiredPermission.ToString();

            var accessType = isAnonymousAllowed
                ? "Anonymous Allowed (Overrides permission)"
                : "Permission Required";

            // ... (Phần log phương thức và route như code cũ của bạn)
            var httpMethods = endpoint.EndpointMetadata
                .OfType<HttpMethodMetadata>()
                .SelectMany(m => m.HttpMethods)
                .Distinct();
            var methods = string.Join(", ", httpMethods);
            if (string.IsNullOrEmpty(methods))
                methods = "N/A";

            var routeTemplate = endpoint.AttributeRouteInfo!.Template;

            _logger.LogInformation("-> {Methods} /{Route} | Required Permission: {Permission} | Access: {Access}",
                methods, routeTemplate, permissionInfo, accessType);

            // 3. Logic kiểm tra quyền và thêm vào PermissionDetector (nếu cần)
            if (requiredPermission != EPermission.Unknown && !isAnonymousAllowed)
            {
                // Lấy chuỗi API/route để thêm vào PermissionDetector
                string apiRoute = $"/{routeTemplate}";
                _logger.LogInformation(apiRoute);

                // **LƯU Ý:** Bạn cần biết EPermission.Administrator được định nghĩa như thế nào 
                // để gọi phương thức AddPermission trong PermissionDetector.
                // Giả sử RequiredPermission là giá trị EPermission bạn cần.

                // Đoạn code này chỉ là ví dụ minh họa cách bạn có thể sử dụng
                // requiredPermission để gọi PermissionDetector.AddPermission
                /*
                requiredPermission.Value.AddPermission(
                    apis: new List<string> { apiRoute }
                    // permissionCodes nếu bạn có
                );
                */
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}