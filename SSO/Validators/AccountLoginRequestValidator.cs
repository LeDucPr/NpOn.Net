using System.Text.RegularExpressions;
using FluentValidation;
using SSO.Requests;

namespace SSO.Validators;

public class AccountLoginRequestValidator : AbstractValidator<AccountLoginRequest>
{
    private AccountLoginRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty()
            .Length(3, 50);
        RuleFor(x => x.Password).NotEmpty()
            .Length(8, 50)
            .Must(HasValidPassword).WithMessage("Password rule invalid");
        RuleFor(x => x.AuthType).NotNull();
    }

    private bool HasValidPassword(string pw)
    {
        var lowercase = new Regex("[a-z]+");
        return (lowercase.IsMatch(pw));
    }

    public static FluentValidation.Results.ValidationResult ValidateRequest(AccountLoginRequest request)
    {
        var validationResult = new AccountLoginRequestValidator().Validate(request);
        return validationResult;
    }
}



public class AccountRefreshTokenValidator : AbstractValidator<AccountRefreshTokenRequest>
{
    private AccountRefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
    public static FluentValidation.Results.ValidationResult ValidateRequest(AccountRefreshTokenRequest request)
    {
        var validationResult = new AccountRefreshTokenValidator().Validate(request);
        return validationResult;
    }
}