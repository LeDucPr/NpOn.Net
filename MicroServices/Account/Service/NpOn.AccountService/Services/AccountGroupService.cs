using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Common.Infrastructures.NpOn.BaseRepository.Postgres;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using MicroServices.Account.Service.NpOn.IAccountService;
using MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;

namespace MicroServices.Account.Service.NpOn.AccountService.Services;

public class AccountGroupService(
    IPostgresBaseRepository baseRepository,
    IAccountGroupStorageAdapter accountGroupStorageAdapter,
    ILogger<CommonService> logger
) : CommonService(logger), IAccountGroupService
{
    private IPostgresBaseRepository _baseRepository = baseRepository;
    public async Task<CommonResponse> GroupAddOrChange(AccountGroupAddOrChangeCommand command)
    {
        return await CommonProcess(async (response) =>
        {
            // only create one record with member = null
            if (command.GroupId == null || command.GroupId == Guid.Empty)
                command.GroupId = IndexerMode.CreateGuid();
            else
            {
                AccountGroupRModel? existedGroup =
                    (await accountGroupStorageAdapter.AccountGroupGetByGroupIds(
                        [command.GroupId.AsDefaultString()], 1, 0))?.First();
                if (existedGroup == null)
                {
                    response.SetFail("AccountGroup Invalid");
                    return;
                }

                if (existedGroup.Leader != command.Leader)
                {
                    response.SetFail("Cannot Change Leader/Director");
                    return;
                }

                if (command.GroupTypes.CombineFlags() != existedGroup.GroupType)
                {
                    response.SetFail("Cannot Change GroupType");
                    return;
                }
            }

            List<AccountGroup> accountGroups = command.FromCommand();
            if (!(await _baseRepository.Merge(accountGroups, true))?.Status ?? false)
            {
                string err = "AccountGroup Add fail";
                if (command.GroupId != null)
                    err = "AccountGroup Change fail";
                response.SetFail(err);
                return;
            }

            response.SetSuccess();
        });
    }

    public async Task<CommonResponse> GroupCopy(AccountGroupCopyCommand command)
    {
        return await CommonProcess(async (response) =>
        {
            int batchSize = 300;
            for (int pageIndex = 0;; pageIndex++)
            {
                List<AccountGroupRModel>? existedAccountGroups =
                    (await accountGroupStorageAdapter.AccountGroupGetByGroupIds(
                        [command.GroupIdNeedCopy.AsDefaultString()], batchSize, pageIndex));
                if (pageIndex == 0 && existedAccountGroups is not { Count: > 0 })
                {
                    response.SetFail("AccountGroup Invalid");
                    return;
                }

                if (existedAccountGroups is not { Count: > 0 })
                    break;

                List<AccountGroup>? accountGroups = command.FromCommand(
                    existedAccountGroups.Select(x => x.Member.AsDefaultGuid()).ToArray());

                if (accountGroups == null)
                {
                    response.SetFail("AccountGroup Copy Invalid");
                    return;
                }

                if (!(await _baseRepository.Merge(accountGroups, true))?.Status ?? false)
                {
                    response.SetFail("AccountGroup Copy fail");
                    return;
                }
            }

            response.SetSuccess();
        });
    }

    public async Task<CommonResponse> Search(AccountGroupSearchQuery query)
    {
        return await CommonProcess(async (response) =>
        {
            // todo: search by account (member) / leader / group name
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse> GroupOrMemberDelete(AccountGroupDeleteCommand command)
    {
        return await CommonProcess(async (response) =>
        {
            // member = null => delete all
            List<AccountGroup> accountGroups = command.FromCommand();
            // if (!(await baseRepository.Delete(accountGroups, command.Members is { Length : > 0 }))?.Status ?? false)
            if (!(await _baseRepository.Delete(accountGroups))?.Status ?? false)
            {
                string err = "AccountGroup Delete fail";
                if (command.Members is { Length : > 0 })
                    err = $"AccountGroup Delete {command.Members.Length} Member fail";
                response.SetFail(err);
                return;
            }

            response.SetSuccess();
        });
    }
}