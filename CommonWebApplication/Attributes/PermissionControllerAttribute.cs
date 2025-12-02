using CommonMode;
using ProjectEnums.AccountEnums;

namespace CommonWebApplication.Attributes;

public class PermissionException : Exception
{
    public PermissionException(string? message) : base()
    {
    }
}

public class PermissionControllerAttribute : Attribute
{
    private EPermission _permission;

    private string[] _permissionCodes = [];

    public PermissionControllerAttribute()
    {
        _permission = EPermission.Unknown;
    }

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

    public PermissionControllerAttribute(EPermission permission, string[] permissionCodes)
    {
        _permission = permission;
        _permissionCodes = _permissionCodes.Concat(permissionCodes).ToArray();
    }

    public bool IsHasPermission(EPermission permissionNeedToCheck, string? permissionCodeNeedToCheck = null)
    {
        if (_permission == EPermission.Unknown || !_permission.HasFlag(permissionNeedToCheck))
            return false;
        if (_permissionCodes is not { Length: > 0 })
            return true;
        if (!string.IsNullOrWhiteSpace(permissionCodeNeedToCheck))
        {
            if (_permissionCodes.Contains(permissionCodeNeedToCheck))
                return true;
            return false;
        }

        return true;
    }

    public EPermission[] GetAllPermission() => _permission.GetFlags();
}