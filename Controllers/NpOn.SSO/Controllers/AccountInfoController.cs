using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Controllers.NpOn.SSO.Mappings.Account;
using Controllers.NpOn.SSO.Requests;
using Controllers.NpOn.SSO.Validators;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Service.NpOn.IAccountService;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.NpOn.SSO.Controllers;

public class AccountInfoController(
    ContextService contextService,
    IAccountInfoService accountInfoService,
    ILogger<AccountController> logger
) : BaseSsoController(logger, contextService)
{
    private readonly ContextService _contextService = contextService;

    [HttpPost]
    public async Task<CommonApiResponse<object>> GetAccountInfo()
    {
        return await ProcessRequest<object>(async (response) =>
        {
            string accountId = _contextService.GetAccountIdAsString() ?? string.Empty;
            if (string.IsNullOrEmpty(accountId))
            {
                response.SetFail("AccountId not null");
                return;
            }

            var accountInfoResponse = await accountInfoService.AccountInfoGetByAccountId(
                new AccountInfoGetByAccountIdQuery
                {
                    AccountId = accountId,
                });

            if (!accountInfoResponse.Status)
            {
                response.SetFail("AccountInfo not found");
                return;
            }

            response.Data = new
            {
                Model = accountInfoResponse.Data?.ToModel(),
            };
            response.SetSuccess();
        });
    }

    [HttpPost]
    public async Task<CommonApiResponse<string>> AddOrChangeAccountInfo(AccountInfoAddOrChangeRequest request)
    {
        return await ProcessRequest<string>(async (response) =>
        {
            var validator = AccountInfoAddOrChangeRequestValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            string accountId = _contextService.GetAccountIdAsString() ?? string.Empty;
            if (string.IsNullOrEmpty(accountId))
            {
                response.SetFail("AccountId not null");
                return;
            }

            var accountInfoResponse = await accountInfoService.AccountInfoAddOrChange(request.ToCommand(accountId));
            if (!accountInfoResponse.Status)
            {
                response.SetFail(request.Id == null || request.Id == Guid.Empty ? "add fail" : "update fail");
                return;
            }

            response.Data = request.Id == null || request.Id == Guid.Empty ? "add success" : "update success";
            response.SetSuccess();
        });
    }
    
    [HttpPost]
    public async Task<CommonApiResponse<string>> AddOrChangeAccountAddress(AccountAddressesAddOrChangeRequest request)
    {
        return await ProcessRequest<string>(async (response) =>
        {
            var validator = AccountAddressesAddOrChangeRequestValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            Guid accountId = _contextService.GetAccountIdAsString().AsDefaultGuid();
            if (accountId == Guid.Empty)
            {
                response.SetFail("AccountId not null");
                return;
            }

            var accountInfoResponse = await accountInfoService.AccountAddressesAddOrChange([request.ToCommand(accountId)]);
            if (!accountInfoResponse.Status)
            {
                response.SetFail("Address Add/Change fail");
                return;
            }

            response.Data = "Address Add/Change success";
            response.SetSuccess();
        });
    }
}