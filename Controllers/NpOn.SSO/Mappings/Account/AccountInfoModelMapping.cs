using Controllers.NpOn.SSO.OutputModels;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

namespace Controllers.NpOn.SSO.Mappings.Account;

public static class AccountInfoModelMapping
{
    public static AccountInfoDetailOutputModel ToModel(this AccountInfoRModel accountInfoRModel)
    {
        return new AccountInfoDetailOutputModel()
        {
            Id = accountInfoRModel.Id,
            AccountId = accountInfoRModel.AccountId,
            CountryId = accountInfoRModel.CountryId,
            ProvinceId = accountInfoRModel.ProvinceId,
            DistrictId = accountInfoRModel.DistrictId,
            WardId = accountInfoRModel.WardId,
            FullName = accountInfoRModel.FullName,
            DateOfBirth = accountInfoRModel.DateOfBirth,
            Gender = accountInfoRModel.Gender,
            Address = accountInfoRModel.Address,
            Occupation = accountInfoRModel.Occupation,
            MaritalStatus = accountInfoRModel.MaritalStatus,
            Bio = accountInfoRModel.Bio,
            Website = accountInfoRModel.Website,
            SocialLinks = accountInfoRModel.SocialLinks,
            IdentificationNumber = accountInfoRModel.IdentificationNumber,
            PassportNumber = accountInfoRModel.PassportNumber,
            TaxCode = accountInfoRModel.TaxCode,
            CompanyName = accountInfoRModel.CompanyName,
            CompanyAddress = accountInfoRModel.CompanyAddress,
            Status = accountInfoRModel.Status,
            CreatedAt = accountInfoRModel.CreatedAt,
            UpdatedAt = accountInfoRModel.UpdatedAt,
        };
    }
}