using ProjectEnums.AccountEnums;

namespace CommonWebApplication.Services;

public static class PermissionDetector
{
    public static void AddPermission(this EPermission permission, List<string>? apis = null,
        List<string>? permissionCodes = null)
    {
        bool exists = Permissions.Any(x => x.Key == permission);
        if (exists)
        {
            var existPermission = Permissions.FirstOrDefault(x => x.Key == permission);
            if (!Permissions.ContainsKey(permission))
                Permissions.Add(permission, (apis, permissionCodes));
            else
            {
                if (existPermission.Value.Apis != null && apis != null)
                    existPermission.Value.Apis.AddRange(apis);
                if (existPermission.Value.PermissionCodes != null && permissionCodes != null)
                    existPermission.Value.PermissionCodes.AddRange(permissionCodes);
            }
        }
    }

    public static void RemovePermission(this EPermission permission)
    {
        Permissions.Remove(permission);
    }

    public static List<string>? GetPermissionCodes(this EPermission permission)
    {
        if (Permissions.Select(x => x.Key).Contains(permission))
            return Permissions[permission].PermissionCodes;
        return null;
    }
    
    public static List<string>? GetApis(this EPermission permission)
    {
        if (Permissions.Select(x => x.Key).Contains(permission))
            return Permissions[permission].Apis;
        return null;
    }

    private static readonly IDictionary<EPermission, (List<string>? Apis, List<string>? PermissionCodes)> Permissions =
        new Dictionary<EPermission, (List<string>? apis, List<string>? permissionCodes)>();
}