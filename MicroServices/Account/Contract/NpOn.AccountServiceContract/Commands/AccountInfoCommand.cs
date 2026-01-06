using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;

[ProtoContract]
public class AccountInfoAddOrChangeCommand : BaseAccountCommand
{
    [ProtoMember(1)] public required string AccountId { get; set; } // acc_srv_account_id ?
    [ProtoMember(6)] public string? FullName { get; set; } // Personal info: Full name
    [ProtoMember(7)] public DateTime? DateOfBirth { get; set; } // Personal info: Date of birth
    [ProtoMember(8)] public EAccountGender? Gender { get; set; } // Personal info: Gender (male/female/other)
    [ProtoMember(10)] public string? Occupation { get; set; } // Personal info: Occupation

    [ProtoMember(11)]
    public string? MaritalStatus { get; set; } // Personal info: Marital status (single/married/divorced)

    [ProtoMember(12)] public string? Bio { get; set; } // Personal info: Bio
    [ProtoMember(13)] public string? Website { get; set; } // Personal info: Website
    [ProtoMember(14)] public string? SocialLinks { get; set; } // Personal info: Social media links (JSON format)
    [ProtoMember(15)] public string? IdentificationNumber { get; set; } // Identification & Legal: ID card number
    [ProtoMember(16)] public string? PassportNumber { get; set; } // Identification & Legal: Passport number
    [ProtoMember(17)] public string? TaxCode { get; set; } // Identification & Legal: Tax code
    [ProtoMember(18)] public string? CompanyName { get; set; } // Identification & Legal: Company name
    [ProtoMember(19)] public string? CompanyAddress { get; set; } // Identification & Legal: Company address
    // [ProtoMember(20)] public Guid? Id { get; set; }
    
    // address
    [ProtoMember(21)] public Guid? CountryId { get; set; }
    [ProtoMember(22)] public Guid? ProvinceId { get; set; }
    [ProtoMember(23)] public Guid? DistrictId { get; set; }
    [ProtoMember(24)] public Guid? WardId { get; set; }
    [ProtoMember(25)] public string? AddressLine { get; set; }
}