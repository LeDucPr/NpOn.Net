using Common.Extensions.NpOn.CommonBaseDomain.Attributes;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.HandleFlow.Attributes;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;

[ProtoContract]
[TableLoader("acc_srv_account_info")]
public sealed class AccountInfo : BaseAccountDomain
{
    [ProtoMember(1)]
    [Field("id")]
    [Pk("id")]
    public Guid? Id { get; set; }

    [ProtoMember(2)]
    [Field("acc_srv_account_id")]
    public Guid AccountId { get; set; }

    // Personal info
    [ProtoMember(3)] [Field("full_name")] public string? FullName { get; set; }

    [ProtoMember(4)]
    [Field("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }

    [ProtoMember(5)] [Field("gender")] public EAccountGender? Gender { get; set; }

    [ProtoMember(6)]
    [Field("occupation")]
    public string? Occupation { get; set; }

    [ProtoMember(7)]
    [Field("marital_status")]
    public string? MaritalStatus { get; set; }

    [ProtoMember(8)] [Field("bio")] public string? Bio { get; set; }
    [ProtoMember(9)] [Field("website")] public string? Website { get; set; }

    [ProtoMember(10)]
    [Field("social_links")]
    public string? SocialLinks { get; set; }

    // Identification & Legal
    [ProtoMember(11)]
    [Field("identification_number")]
    public string? IdentificationNumber { get; set; }

    [ProtoMember(12)]
    [Field("passport_number")]
    public string? PassportNumber { get; set; }

    [ProtoMember(13)] [Field("tax_code")] public string? TaxCode { get; set; }

    [ProtoMember(14)]
    [Field("company_name")]
    public string? CompanyName { get; set; }

    [ProtoMember(15)]
    [Field("company_address")]
    public string? CompanyAddress { get; set; }

    // Management & Status
    [ProtoMember(16)] [Field("status")] public EAccountInfoStatus Status { get; set; }

    [ProtoMember(17)]
    [Field("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ProtoMember(18)]
    [Field("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public AccountInfo()
    {
    }

    public AccountInfo(AccountInfoRModel accountInfoObj)
    {
        Id = accountInfoObj.Id;
        AccountId = accountInfoObj.AccountId;
        FullName = accountInfoObj.FullName;
        DateOfBirth = accountInfoObj.DateOfBirth;
        Gender = accountInfoObj.Gender;
        Occupation = accountInfoObj.Occupation;
        MaritalStatus = accountInfoObj.MaritalStatus;
        Bio = accountInfoObj.Bio;
        Website = accountInfoObj.Website;
        SocialLinks = accountInfoObj.SocialLinks;
        IdentificationNumber = accountInfoObj.IdentificationNumber;
        PassportNumber = accountInfoObj.PassportNumber;
        TaxCode = accountInfoObj.TaxCode;
        CompanyName = accountInfoObj.CompanyName;
        CompanyAddress = accountInfoObj.CompanyAddress;
        Status = EAccountInfoStatus.Active;
        CreatedAt = accountInfoObj.CreatedAt;
        UpdatedAt = accountInfoObj.UpdatedAt;
    }

    public AccountInfo(AccountInfoAddOrChangeCommand command)
    {
        AccountId = Guid.Parse(command.AccountId);
        FullName = command.FullName;
        DateOfBirth = command.DateOfBirth;
        Gender = command.Gender;
        Occupation = command.Occupation;
        MaritalStatus = command.MaritalStatus;
        Bio = command.Bio;
        Website = command.Website;
        SocialLinks = command.SocialLinks;
        IdentificationNumber = command.IdentificationNumber;
        PassportNumber = command.PassportNumber;
        TaxCode = command.TaxCode;
        CompanyName = command.CompanyName;
        CompanyAddress = command.CompanyAddress;
        Status = EAccountInfoStatus.Active;
        UpdatedAt = DateTime.Now;
        CreatedAt = DateTime.Now;
    }

    public AccountInfo(AccountSyncFromOldSystemCommand command)
    {
        Id = IndexerMode.CreateGuid();
        Change(command);
    }

    public void ChangeAccountInfoStatus(EAccountInfoStatus? accountInfoStatus = null)
    {
        Status = accountInfoStatus ?? EAccountInfoStatus.Deleted;
    }
    
    public void Change(AccountSyncFromOldSystemCommand command)
    {
        AccountId = command.AccountId;
        FullName = command.FullName;
        DateOfBirth = command.DateOfBirth;
        Gender = command.Gender;
        Occupation = command.Occupation;
        MaritalStatus = command.Marital;
        Bio = command.Bio;
        Website = command.Website;
        SocialLinks = command.SocialLinks;
        IdentificationNumber = command.IdentificationNumber;
        PassportNumber = command.PassportNumber;
        TaxCode = command.TaxCode;
        CompanyName = command.CompanyName;
        CompanyAddress = command.CompanyAddress;
        Status = EAccountInfoStatus.Active;
        CreatedAt = command.CreatedAt;
        UpdatedAt = DateTime.Now;
    }
}