using Common.Extensions.NpOn.CommonMode;
using Definitions.NpOn.ProjectEnums.AccountEnums;

namespace Common.Extensions.NpOn.CommonWebApplication.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class PermissionControllerAttribute : Attribute
{
    private readonly EPermission _permission;

    private readonly string[] _permissionCodes = [];

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
        if (_permission == EPermission.Unknown)
            return false;
        if (permissionNeedToCheck == EPermission.Unknown)
            return false;
        // many permissions in one account 
        bool isHasFlag = false;
        var checkSet = permissionNeedToCheck.GetFlags().ToHashSet().Exclude([EPermission.Unknown]);
        EPermission[] allSelfPermission = GetAllPermission().Exclude([EPermission.Unknown]);
        var commonPermissions =
            allSelfPermission
                .Where(p => checkSet.Contains(p))
                .ToArray();
        if (commonPermissions is { Length: > 0 })
            isHasFlag = true;

        if (isHasFlag && _permissionCodes is not { Length: > 0 })
            isHasFlag = true;

        if (isHasFlag && !string.IsNullOrWhiteSpace(permissionCodeNeedToCheck))
            return _permissionCodes.Contains(permissionCodeNeedToCheck);

        return isHasFlag;
    }

    public EPermission GetAllPermissionAsOne => _permission;
    public EPermission[] GetAllPermission() => _permission.GetFlags();
}