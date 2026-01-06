using Definitions.NpOn.ProjectEnums.AccountEnums;

namespace Controllers.NpOn.SSO.OutputModels;

public class AccountInfoDetailOutputModel
{
    public Guid Id { get; set; }
    public required Guid AccountId { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? DistrictId { get; set; }
    public Guid? WardId { get; set; }
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public EAccountGender? Gender { get; set; }
    public string? Address { get; set; }
    public string? Occupation { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? SocialLinks { get; set; }
    public string? IdentificationNumber { get; set; }
    public string? PassportNumber { get; set; }
    public string? TaxCode { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public int Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}