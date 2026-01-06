
using Definitions.NpOn.ProjectEnums.AccountEnums;

namespace Controllers.NpOn.SSO.Requests;

public class AccountPermissionAddOrChangeRequest
{
    public required Guid AccountId { get; set; }
    public required AccountPermissionAddOrChangeControllerRequest[] Controllers { get; set; }
}

public class AccountPermissionAddOrChangeControllerRequest
{
    public required string ControllerCode { get; set; }
    public EPermissionAccessController AccessPermission { get; set; }
}

public class AccountPermissionExceptionAddOrChangeManyRequest
{
    public Guid[]? AccountIds { get; set; }
    public Guid[]? GroupIds { get; set; }
    public AccountPermissionExceptionAddOrChangeManyControllerCodeRequest[]? Controllers { get; set; }
}

public class AccountPermissionExceptionAddOrChangeManyControllerCodeRequest
{
    public required string ControllerCode { get; set; }
    public EPermissionAccessController AccessPermission { get; set; }
}

public class AccountSyncFromOldSystemRequest
{
    public required Guid AccountId { get; set; }
    public required string Md5HashPassword { get; set; }
    public required string UserName { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? AvatarUrl { get; set; }
    public EPermission? Permission { get; set; }
    public string? Email { get; set; }
    public EAccountStatus? AccountStatus { get; set; }

    // address
    public Guid? CountryId { get; set; } // this system
    public Guid? ProvinceId { get; set; } // this system
    public Guid? DistrictId { get; set; } // this system
    public Guid? WardId { get; set; } // this system
    public string? AddressLine { get; set; }
    public EAddressType? AddressType { get; set; }

    // info
    public EAccountGender? Gender { get; set; }
    public string? Occupation { get; set; }
    public string? Marital { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? SocialLinks { get; set; }
    public string? IdentificationNumber { get; set; }
    public string? PassportNumber { get; set; }
    public string? TaxCode { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public DateTime? DateOfBirth { get; set; }
}