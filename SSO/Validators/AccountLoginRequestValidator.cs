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
        ;
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