using Common.Extensions.NpOn.CommonWebApplication.Attributes;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Service.NpOn.IAccountService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Common.Extensions.NpOn.CommonWebApplication.Services;

public class PermissionService(
    ContextService contextService,
    IAccountPermissionService accountPermissionService,
    IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
    ILogger<PermissionService> logger)
{
    public async Task<bool> AutoSyncPermissionController()
    {
        var endpoints = actionDescriptorCollectionProvider.ActionDescriptors.Items
            .Where(ad => ad.AttributeRouteInfo != null) // Only take endpoint has route attribute
            .ToList();
        var cad = actionDescriptorCollectionProvider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Where(cad => cad.AttributeRouteInfo != null)
            .ToList().FirstOrDefault();

        string? cadName = cad?.ControllerTypeInfo.Assembly.GetName().Name;
        if (string.IsNullOrEmpty(cadName))
        {
            string err = "No ControllerActionDescriptor found.";
            logger.LogInformation(err);
            throw new Exception(err);
        }

        var syncTasks = new List<Task>();
        Guid controllerStartVersionId = Guid.NewGuid();
        List<AccountPermissionControllerAddOrChangeCommand> commands = [];
        foreach (var endpoint in endpoints)
        {
            bool isAnonymousAllowed = endpoint.EndpointMetadata
                .Any(m => m.GetType() == typeof(AllowAnonymousAttribute));
            if (isAnonymousAllowed)
                continue;

            PermissionControllerAttribute? permissionControllerAttribute = endpoint.EndpointMetadata
                .OfType<PermissionControllerAttribute>()
                .FirstOrDefault();

            PermissionRequiredAttribute? permissionRequiredAttribute = endpoint.EndpointMetadata
                .OfType<PermissionRequiredAttribute>()
                .FirstOrDefault();


            var action = endpoint.RouteValues["action"];
            var controller = endpoint.RouteValues["controller"];
            if (controller == null || action == null)
                continue;
            string controllerCode = contextService.GenControllerCodeFormula(cadName, controller, action);

            if (permissionControllerAttribute != null)
            {
                if (permissionRequiredAttribute == null)
                {
                    string err =
                        $"Hey fucking bro! You need to ADD [PermissionRequiredAttribute] before [PermissionControllerAttribute in {controllerCode}";
                    logger.LogError(err);
                    throw new Exception(err);
                }

                if (permissionControllerAttribute.IsHasPermission(EPermission.Unknown))
                {
                    string err =
                        $"Hey fucking bro! You need to DELETE - EPermission.Unknown [PermissionControllerAttribute in {controllerCode} or ADD Others Permission in {controllerCode}";
                    logger.LogError(err);
                    throw new Exception(err);
                }
            }
            else
                continue;

            // var httpMethods = endpoint.EndpointMetadata.OfType<HttpMethodMetadata>().SelectMany(m => m.HttpMethods).Distinct();

            AccountPermissionControllerAddOrChangeCommand command = new AccountPermissionControllerAddOrChangeCommand
            {
                ControllerName = controllerCode, // code
                Permission = permissionControllerAttribute.GetAllPermissionAsOne,
                HostCode = cadName,
                VersionId = controllerStartVersionId,
                Description = permissionRequiredAttribute.Description,
            };
            commands.Add(command);
            if (commands.Count == 500)
            {
                syncTasks.Add(StartSyncPermissionWithController(commands.ToArray()));
                commands = [];
            }
        }

        if (commands.Count > 0)
            syncTasks.Add(StartSyncPermissionWithController(commands.ToArray()));

        logger.LogInformation("Waiting for all {TaskCount} permission sync batches to complete...", syncTasks.Count);
        await Task.WhenAll(syncTasks);
        logger.LogInformation("All permission sync batches completed successfully.");
        await SyncClearPermissionWithController(controllerStartVersionId, cadName);
        return true;
    }

    private Task StartSyncPermissionWithController(AccountPermissionControllerAddOrChangeCommand[] commands)
    {
        return Task.Run(async () =>
        {
            logger.LogInformation("Starting to sync a batch of {CommandCount} permissions...", commands.Length);
            await accountPermissionService.SyncPermissionWithController(commands);
            logger.LogInformation("Sync batch finished.");
        });
    }

    private Task SyncClearPermissionWithController(Guid currentVersionId, string hostCode)
    {
        return Task.Run(async () =>
        {
            logger.LogInformation(
                "Starting to clear old permission versions for host '{HostCode}' except for current version '{CurrentVersionId}'.",
                hostCode, currentVersionId);
            await accountPermissionService.ClearOldControllersVersion(
                new AccountPermissionControllerDeleteByHostCodeAndVersionIdCommand()
                {
                    HostCode = hostCode,
                    VersionId = currentVersionId,
                });
            logger.LogInformation("Finished clearing old permission versions.");
        });
    }
}