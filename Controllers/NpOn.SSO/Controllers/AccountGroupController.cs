using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Controllers.NpOn.SSO.Requests;
using Controllers.NpOn.SSO.Validators;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Service.NpOn.IAccountService;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.NpOn.SSO.Controllers;

public class AccountGroupController(
    ILogger<AccountController> logger,
    ContextService contextService,
    // IAuthenticationService authenticationService,
    // IAccountPermissionService accountPermissionService,
    IAccountGroupService accountGroupService
) : BaseSsoController(logger, contextService)
{
    [HttpPost]
    public async Task<CommonApiResponse<string>> AddOrChangeGroupAndMember(
        [FromBody] AccountGroupAddOrChangeRequest request)
    {
        return await ProcessRequest<string>(async (response) =>
        {
            var validator = AccountGroupAddOrChangeRequestValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            var addOrChangeResponse = await accountGroupService.GroupAddOrChange(new AccountGroupAddOrChangeCommand()
            {
                GroupId = request.GroupId.AsDefaultGuid(),
                Leader = request.Leader,
                Members = request.Members,
                GroupName = request.GroupName,
                GroupTypes = request.GroupTypes
            });

            if (!addOrChangeResponse.Status)
            {
                response.SetFail("Add Or Change Group fail");
                return;
            }

            response.Data = "Add Or Change Group Success";
            response.SetSuccess();
        });
    }

    [HttpPost]
    public async Task<CommonApiResponse<string>> GroupCopy([FromBody] AccountGroupCopyRequest request)
    {
        return await ProcessRequest<string>(async (response) =>
        {
            var validator = AccountGroupCopyRequestValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            var addOrChangeResponse = await accountGroupService.GroupCopy(new AccountGroupCopyCommand
            {
                GroupIdNeedCopy = request.GroupIdNeedCopy,
                Components = request.Components.Select(x => new AccountGroupCopyComponentCommand
                {
                    Leader = x.Leader,
                    GroupName = x.GroupName,
                    GroupTypes = x.GroupTypes,
                    MemberAdds = x.MemberAdds,
                    MemberExcludes = x.MemberExcludes,
                }).ToArray(),
            });

            if (!addOrChangeResponse.Status)
            {
                response.SetFail("(Delete Group)/(Delete Group Or Member) fail");
                return;
            }

            response.Data = "Add New Groups Success";
            response.SetSuccess();
        });
    }

    [HttpPost]
    public async Task<CommonApiResponse<string>> DeleteGroupOrMember([FromBody] AccountGroupDeleteRequest request)
    {
        return await ProcessRequest<string>(async (response) =>
        {
            var validator = AccountGroupDeleteRequestValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            var addOrChangeResponse = await accountGroupService.GroupOrMemberDelete(new AccountGroupDeleteCommand()
            {
                GroupId = request.GroupId,
                Leader = request.Leader,
                Members = request.Members,
                GroupName = request.GroupName
            });

            if (!addOrChangeResponse.Status)
            {
                response.SetFail("(Delete Group)/(Delete Group Or Member) fail");
                return;
            }

            response.Data = "Delete Group Success";
            response.SetSuccess();
        });
    }
}