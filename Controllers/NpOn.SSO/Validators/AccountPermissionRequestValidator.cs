using Controllers.NpOn.SSO.Requests;
using FluentValidation;

namespace Controllers.NpOn.SSO.Validators;

public class AccountPermissionRequestValidator : AbstractValidator<AccountPermissionAddOrChangeRequest>
{
    private AccountPermissionRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Controllers).NotEmpty();
    }

    public static FluentValidation.Results.ValidationResult ValidateRequest(AccountPermissionAddOrChangeRequest request)
    {
        var validationResult = new AccountPermissionRequestValidator().Validate(request);
        return validationResult;
    }
}

public class AccountPermissionExceptionAddOrChangeManyRequestValidator
    : AbstractValidator<AccountPermissionExceptionAddOrChangeManyRequest>
{
    private AccountPermissionExceptionAddOrChangeManyRequestValidator()
    {
        RuleFor(x => x.Controllers)
            .Must(c => c!.Length > 0);

        RuleFor(x => x)
            .Must(x =>
                (x.AccountIds != null && x.AccountIds.Length > 0) ||
                (x.GroupIds != null && x.GroupIds.Length > 0)
            )
            .WithMessage("At least one of AccountIds or GroupIds must be provided (non-empty).");
    }

    public static FluentValidation.Results.ValidationResult ValidateRequest(
        AccountPermissionExceptionAddOrChangeManyRequest request)
    {
        var validationResult = new AccountPermissionExceptionAddOrChangeManyRequestValidator().Validate(request);
        return validationResult;
    }
}