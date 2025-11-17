using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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
        _logger.LogInformation("Starting to list all registered API endpoints...");

        var endpoints = _actionDescriptorCollectionProvider.ActionDescriptors.Items
            .Where(ad => ad.AttributeRouteInfo != null) // Chỉ lấy các endpoint có route attribute
            .ToList();

        if (!endpoints.Any())
        {
            _logger.LogWarning("No API endpoints with attribute routes found.");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Discovered Endpoints:");
        foreach (var endpoint in endpoints)
        {
            // Lấy phương thức HTTP (GET, POST, etc.)
            var httpMethods = endpoint.EndpointMetadata
                                      .OfType<HttpMethodMetadata>()
                                      .SelectMany(m => m.HttpMethods)
                                      .Distinct();

            var methods = string.Join(", ", httpMethods);
            if (string.IsNullOrEmpty(methods))
            {
                methods = "N/A";
            }

            // Lấy template của route
            var routeTemplate = endpoint.AttributeRouteInfo!.Template;

            _logger.LogInformation("-> {Methods} /{Route}", methods, routeTemplate);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
