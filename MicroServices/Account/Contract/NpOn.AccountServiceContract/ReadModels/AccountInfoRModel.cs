using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

[ProtoContract]
public class AccountInfoRModel : BaseAccountRModelFromGrpcTable
{
    [ProtoMember(1)] public required Guid AccountId { get; set; }
    [ProtoMember(2)] public Guid? CountryId { get; set; }
    [ProtoMember(3)] public Guid? ProvinceId { get; set; }
    [ProtoMember(4)] public Guid? DistrictId { get; set; }
    [ProtoMember(5)] public Guid? WardId { get; set; }
    [ProtoMember(6)] public string? FullName { get; set; }
    [ProtoMember(7)] public DateTime? DateOfBirth { get; set; }
    [ProtoMember(8)] public EAccountGender? Gender { get; set; }
    [ProtoMember(9)] public string? Address { get; set; }
    [ProtoMember(10)] public string? Occupation { get; set; }
    [ProtoMember(11)] public string? MaritalStatus { get; set; }
    [ProtoMember(12)] public string? Bio { get; set; }
    [ProtoMember(13)] public string? Website { get; set; }
    [ProtoMember(14)] public string? SocialLinks { get; set; }
    [ProtoMember(15)] public string? IdentificationNumber { get; set; }
    [ProtoMember(16)] public string? PassportNumber { get; set; }
    [ProtoMember(17)] public string? TaxCode { get; set; }
    [ProtoMember(18)] public string? CompanyName { get; set; }
    [ProtoMember(19)] public string? CompanyAddress { get; set; }
    [ProtoMember(20)] public int Status { get; set; }
    [ProtoMember(21)] public Guid Id { get; set; }
    
    [ProtoMember(22)] public AccountAddressRModel[]? Addresses { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(AccountId), "acc_srv_account_id");
        FieldMap.Add(nameof(Id), "id");
        FieldMap.Add(nameof(FullName), "full_name");
        FieldMap.Add(nameof(DateOfBirth), "date_of_birth");
        FieldMap.Add(nameof(Gender), "gender");
        FieldMap.Add(nameof(Occupation), "occupation");
        FieldMap.Add(nameof(MaritalStatus), "marital_status");
        FieldMap.Add(nameof(Bio), "bio");
        FieldMap.Add(nameof(Website), "website");
        FieldMap.Add(nameof(SocialLinks), "social_links");
        FieldMap.Add(nameof(IdentificationNumber), "identification_number");
        FieldMap.Add(nameof(PassportNumber), "passport_number");
        FieldMap.Add(nameof(TaxCode), "tax_code");
        FieldMap.Add(nameof(CompanyName), "company_name");
        FieldMap.Add(nameof(CompanyAddress), "company_address");
        FieldMap.Add(nameof(Status), "status");
    }
}