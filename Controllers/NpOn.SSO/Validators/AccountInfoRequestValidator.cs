using Controllers.NpOn.SSO.Requests;
using FluentValidation;

namespace Controllers.NpOn.SSO.Validators;

public class AccountInfoAddOrChangeRequestValidator : AbstractValidator<AccountInfoAddOrChangeRequest>
{
    private AccountInfoAddOrChangeRequestValidator()
    {
    }

    public static FluentValidation.Results.ValidationResult ValidateRequest(AccountInfoAddOrChangeRequest request)
    {
        var validationResult = new AccountInfoAddOrChangeRequestValidator().Validate(request);
        return validationResult;
    }
}

public class AccountAddressesAddOrChangeRequestValidator : AbstractValidator<AccountAddressesAddOrChangeRequest>
{
    private AccountAddressesAddOrChangeRequestValidator()
    {
    }

    public static FluentValidation.Results.ValidationResult ValidateRequest(AccountAddressesAddOrChangeRequest request)
    {
        var validationResult = new AccountAddressesAddOrChangeRequestValidator().Validate(request);
        return validationResult;
    }
}