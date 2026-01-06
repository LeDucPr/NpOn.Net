using Controllers.NpOn.SSO.Requests;
using FluentValidation;

namespace Controllers.NpOn.SSO.Validators;

public class AccountGroupAddOrChangeRequestValidator : AbstractValidator<AccountGroupAddOrChangeRequest>
{
    private AccountGroupAddOrChangeRequestValidator()
    {
        RuleFor(x => x.Leader).NotEmpty();
    }

    public static FluentValidation.Results.ValidationResult ValidateRequest(AccountGroupAddOrChangeRequest request)
    {
        var validationResult = new AccountGroupAddOrChangeRequestValidator().Validate(request);
        return validationResult;
    }
}

public class AccountGroupCopyRequestValidator : AbstractValidator<AccountGroupCopyRequest>
{
    public AccountGroupCopyRequestValidator()
    {
        RuleFor(x => x.Components).NotNull().NotEmpty();
        RuleForEach(x => x.Components).ChildRules(component =>
        {
            component.RuleFor(c => c.Leader).NotEmpty();
            component.RuleFor(c => c.GroupTypes).NotNull().NotEmpty();
        });
    }

    public static FluentValidation.Results.ValidationResult ValidateRequest(AccountGroupCopyRequest request)
    {
        var validationResult = new AccountGroupCopyRequestValidator().Validate(request);
        return validationResult;
    }
}

public class AccountGroupDeleteRequestValidator : AbstractValidator<AccountGroupDeleteRequest>
{
    private AccountGroupDeleteRequestValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.Leader).NotEmpty();
    }

    public static FluentValidation.Results.ValidationResult ValidateRequest(AccountGroupDeleteRequest request)
    {
        var validationResult = new AccountGroupDeleteRequestValidator().Validate(request);
        return validationResult;
    }
}