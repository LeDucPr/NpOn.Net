using ProjectEnums.AccountEnums;

namespace CommonWebApplication.Services;

public static class PermissionDetector
{
    public static void AddPermission(this EPermission permission, List<string> permissionCodes)
    {
        bool exists = Permissions.Any(x => x.Key == permission);
        if (exists)
        {
            var existPermission = Permissions.FirstOrDefault(x => x.Key == permission);
            if (existPermission.Value != null)
                existPermission.Value.AddRange(permissionCodes);
            else
                Permissions.Add(permission, permissionCodes);
        }
    }

    public static void RemovePermission(this EPermission permission)
    {
        if (Permissions.ContainsKey(permission))
            Permissions.Remove(permission);
    }

    private static readonly IDictionary<EPermission, List<string>> Permissions =
        new Dictionary<EPermission, List<string>>();
}

public class PermissionService
{
    public PermissionService(
        ILogger<PermissionService> logger,
        IHttpContextAccessor? httpContextAccessor,
        ContextService contextService,
        IServiceProvider serviceProvider,
        // AuthenService authenService,
        ILogAction logAction)
    {
        Init();
    }

    private void Init()
    {
        
    }
}