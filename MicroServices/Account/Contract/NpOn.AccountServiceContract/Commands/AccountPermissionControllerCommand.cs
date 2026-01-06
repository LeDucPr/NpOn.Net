using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;

[ProtoContract]
public class AccountPermissionControllerAddOrChangeCommand : BaseAccountCommand
{
    [ProtoMember(1)] public required string HostCode { get; set; }
    [ProtoMember(2)] public required Guid VersionId { get; set; }
    [ProtoMember(3)] public required string ControllerName { get; set; }
    public string Code => ControllerName;
    [ProtoMember(4)] public EPermission Permission { get; set; } // flags
    [ProtoMember(5)] public string? Description { get; set; }
}

[ProtoContract]
public class AccountPermissionControllerDeleteByHostCodeAndVersionIdCommand : BaseAccountCommand
{
    [ProtoMember(1)] public required string HostCode { get; set; }
    [ProtoMember(2)] public required Guid VersionId { get; set; }
}

[ProtoContract]
public class AccountPermissionExceptionAddOrChangeCommand : BaseAccountCommand
{
    [ProtoMember(1)] public required Guid AccountId { get; set; }
    [ProtoMember(2)] public required string ControllerCode { get; set; }
    [ProtoMember(3)] public EPermissionAccessController AccessPermission { get; set; }
}

[ProtoContract]
public class AccountPermissionExceptionAddOrChangeManyCommand : BaseAccountCommand
{
    [ProtoMember(1)] public Guid[]? AccountIds { get; set; }
    [ProtoMember(2)] public Guid[]? GroupIds { get; set; }

    [ProtoMember(3)]
    public AccountPermissionExceptionAddOrChangeManyControllerCodeCommand[]? ControllerComponents { get; set; }
}

[ProtoContract]
public class AccountPermissionExceptionAddOrChangeManyControllerCodeCommand
{
    [ProtoMember(1)] public required string ControllerCode { get; set; }
    [ProtoMember(2)] public EPermissionAccessController AccessPermission { get; set; }
}

[ProtoContract]
public class AccountSyncFromOldSystemCommand : BaseAccountCommand
{
    [ProtoMember(1)] public required Guid AccountId { get; set; }
    [ProtoMember(2)] public required string UserName { get; set; }
    [ProtoMember(3)] public required string HashPassword { get; set; }
    [ProtoMember(4)] public string? FullName { get; set; }
    [ProtoMember(5)] public string? PhoneNumber { get; set; }
    [ProtoMember(6)] public DateTime? CreatedAt { get; set; }
    [ProtoMember(7)] public string? AvatarUrl { get; set; }
    [ProtoMember(8)] public EPermission? Permission { get; set; }
    [ProtoMember(9)] public string? Email { get; set; }
    [ProtoMember(10)] public EAccountStatus? AccountStatus { get; set; }

    // address
    [ProtoMember(11)] public Guid? CountryId { get; set; } // this system
    [ProtoMember(12)] public Guid? ProvinceId { get; set; } // this system
    [ProtoMember(13)] public Guid? DistrictId { get; set; } // this system
    [ProtoMember(14)] public Guid? WardId { get; set; } // this system
    [ProtoMember(15)] public string? AddressLine { get; set; }
    [ProtoMember(16)] public EAddressType AddressType { get; set; }

    // account info 
    [ProtoMember(17)] public EAccountGender? Gender { get; set; }
    [ProtoMember(18)] public string? Occupation { get; set; }
    [ProtoMember(19)] public string? Marital { get; set; }
    [ProtoMember(20)] public string? Bio { get; set; }
    [ProtoMember(21)] public string? Website { get; set; }
    [ProtoMember(22)] public string? SocialLinks { get; set; }
    [ProtoMember(23)] public string? IdentificationNumber { get; set; }
    [ProtoMember(24)] public string? PassportNumber { get; set; }
    [ProtoMember(25)] public string? TaxCode { get; set; }
    [ProtoMember(26)] public string? CompanyName { get; set; }
    [ProtoMember(27)] public string? CompanyAddress { get; set; }
    [ProtoMember(28)] public DateTime? DateOfBirth { get; set; }
}