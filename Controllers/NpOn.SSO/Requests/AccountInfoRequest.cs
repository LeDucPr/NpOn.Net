using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;

namespace Controllers.NpOn.SSO.Requests;

public class AccountInfoAddOrChangeRequest
{
    public Guid? Id { get; set; }
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public EAccountGender? Gender { get; set; }
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

    // address (default)
    public Guid? CountryId { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? DistrictId { get; set; }
    public Guid? WardId { get; set; }
    public string? AddressLine { get; set; }
}

public class AccountAddressesAddOrChangeRequest
{
    public Guid? CountryId { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? DistrictId { get; set; }
    public Guid? WardId { get; set; }
    public string? AddressLine { get; set; }
    public EAddressType AddressType { get; set; }
}

public static class AccountInfoAddOrChangeRequestExtensions
{
    public static AccountInfoAddOrChangeCommand ToCommand(this AccountInfoAddOrChangeRequest request, string accountIdString)
    {
        return new AccountInfoAddOrChangeCommand
        {
            // Id = request.Id,
            AccountId = accountIdString,
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Occupation = request.Occupation,
            MaritalStatus = request.MaritalStatus,
            Bio = request.Bio,
            Website = request.Website,
            SocialLinks = request.SocialLinks,
            IdentificationNumber = request.IdentificationNumber,
            PassportNumber = request.PassportNumber,
            TaxCode = request.TaxCode,
            CompanyName = request.CompanyName,
            CompanyAddress = request.CompanyAddress,
            CountryId = request.CountryId,
            ProvinceId = request.ProvinceId,
            DistrictId = request.DistrictId,
            WardId = request.WardId,
            AddressLine = request.AddressLine,
        };
    }

    public static AccountAddressAddOrChangeCommand ToCommand(this AccountAddressesAddOrChangeRequest request, Guid accountId)
    {
        return new AccountAddressAddOrChangeCommand()
        {
            AccountId = accountId,
            CountryId = request.CountryId,
            ProvinceId = request.ProvinceId,
            DistrictId = request.DistrictId,
            WardId = request.WardId,
            AddressLine = request.AddressLine,
            AddressType = request.AddressType,

        };
    }
}