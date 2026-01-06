using Common.Extensions.NpOn.CommonMode;
using Definitions.NpOn.ProjectEnums.AccountEnums;

namespace Common.Extensions.NpOn.CommonWebApplication.Attributes;

public class PermissionRequiredAttribute : Attribute
{
    public string Description { get; }
    private readonly EPermission _permission;

    public PermissionRequiredAttribute(string? description = null)
    {
        Description = description ?? string.Empty;
        _permission = EPermission.Unknown;
    }

    /// <summary>
    /// The basic permission required to access and use this API
    /// </summary>
    /// <param name="description"></param>
    /// <param name="permissions"></param>
    public PermissionRequiredAttribute(string description, params EPermission[] permissions)
    {
        Description = description;
        _permission = permissions.Aggregate(EPermission.Unknown, (current, p) => current | p);
    }

    public bool IsHasPermission(EPermission permissionNeedToCheck)
    {
        if (_permission == EPermission.Unknown)
            return true;
        if (permissionNeedToCheck == EPermission.Unknown)
            return true;
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
        return isHasFlag;
    }

    private EPermission[] GetAllPermission() => _permission.GetFlags();
}