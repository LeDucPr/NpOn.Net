using System.Text.RegularExpressions;
using Controllers.NpOn.SSO.Requests;
using FluentValidation;

namespace Controllers.NpOn.SSO.Validators;

public class AccountSignupRequestValidator : AbstractValidator<AccountSignupRequest>
{
    private AccountSignupRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FullName).NotEmpty();
        RuleFor(x => x.UserName).NotEmpty().Length(3, 50);
        RuleFor(x => x.Password).NotEmpty().Length(8, 50).Must(HasValidPassword);
        RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^((\+?)|0)\d{9}$");
    }

    private bool HasValidPassword(string pw)
    {
        if (string.IsNullOrWhiteSpace(pw)) return false;

        var hasLowercase = new Regex(@"[a-z]+");
        var hasUppercase = new Regex(@"[A-Z]+");
        var hasDigit = new Regex(@"[0-9]+");
        var hasSpecialChar = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

        return hasLowercase.IsMatch(pw) &&
               hasUppercase.IsMatch(pw) &&
               hasDigit.IsMatch(pw) &&
               hasSpecialChar.IsMatch(pw);
    }

    public static FluentValidation.Results.ValidationResult ValidateRequest(AccountSignupRequest request)
    {
        var validationResult = new AccountSignupRequestValidator().Validate(request);
        return validationResult;
    }
}

public class AccountLoginRequestValidator : AbstractValidator<AccountLoginRequest>
{
    private AccountLoginRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().Length(3, 50);
        // RuleFor(x => x.Password).NotEmpty().Length(8, 50).Must(HasValidPassword);
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


public class ChangeAccountPasswordRequestValidator : AbstractValidator<ChangeAccountPasswordRequest>
{
    private ChangeAccountPasswordRequestValidator()
    {
        RuleFor(x =>x.AccountId).NotEmpty();
    }

    public static FluentValidation.Results.ValidationResult ValidateRequest(ChangeAccountPasswordRequest request)
    {
        var validationResult = new ChangeAccountPasswordRequestValidator().Validate(request);
        return validationResult;
    }
}