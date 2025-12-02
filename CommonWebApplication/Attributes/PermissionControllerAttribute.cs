using CommonMode;
using ProjectEnums.AccountEnums;

namespace CommonWebApplication.Attributes;

public class PermissionControllerAttribute : Attribute
{
    private EPermission _permission;

    private string[]? _permissionCodes = [];

    public PermissionControllerAttribute(EPermission permission)
    {
        _permission = permission;
    }

    public PermissionControllerAttribute(params EPermission[]? permissions)
    {
        _permission = 0;
        if (permissions is not { Length: > 0 })
            return;
        foreach (EPermission permission in permissions)
        {
            _permission |= permission;
        }
    }

    // public PermissionControllerAttribute(Dictionary<EPermission, IEnumerable<string>> groupPermissions)
    // {
    // }

    public EPermission[] GetAllPermission() => _permission.GetFlags();
}