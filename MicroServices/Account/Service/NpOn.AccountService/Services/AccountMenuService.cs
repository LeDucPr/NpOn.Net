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

public class AccountMenuService(
    IPostgresBaseRepository baseRepository,
    IAccountMenuStorageAdapter accountMenuStorageAdapter,
    ILogger<CommonService> logger) : CommonService(logger), IAccountMenuService
{
    public async Task<CommonResponse> AddOrChangeAccountMenu(AccountMenuAddOrChangeCommand command)
    {
        return await CommonProcess(async (response) =>
        {
            if (command.Id == null || command.Id == Guid.Empty)
            {
                var accountMenu = new AccountMenu(command);
                if (!(await baseRepository.Add([accountMenu]))?.Status ?? false)
                {
                    response.SetFail("Add account menu fail");
                    return;
                }
            }
            else
            {
                var accountMenuRModel = await accountMenuStorageAdapter
                    .AccountMenuGetById(new AccountMenuGetByIdQuery
                        { Id = command.Id.AsDefaultGuid() });
                if (accountMenuRModel == null)
                {
                    response.SetFail("Account menu not existed. Change fail");
                    return;
                }

                var changeAccountMenu = new AccountMenu(accountMenuRModel);
                changeAccountMenu.Change(command);
                if (!(await baseRepository
                        .Update([changeAccountMenu]))?.Status ?? false)
                {
                    response.SetFail("Change account menu fail");
                    return;
                }
            }

            response.SetSuccess();
        });
    }

    public async Task<CommonResponse> DeleteAccountMenu(AccountMenuDeleteCommand command)
    {
        return await CommonProcess(async (response) =>
        {
            var accountMenuRModel =
                await accountMenuStorageAdapter.AccountMenuGetById(
                    new AccountMenuGetByIdQuery { Id = command.Id.AsDefaultGuid() });
            if (accountMenuRModel == null)
            {
                response.SetFail("Account menu not existed. Delete fail");
                return;
            }

            var accountMenu = new AccountMenu(accountMenuRModel);
            accountMenu.Delete();
            if (!(await baseRepository.Update([accountMenu]))?.Status ?? false)
            {
                response.SetFail("Delete account menu fail");
                return;
            }

            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<AccountMenuRModel?>> GetByIdMenu(
        AccountMenuGetByIdQuery query)
    {
        return await CommonProcess<AccountMenuRModel?>(async (response) =>
        {
            var accountMenuRModel =
                await accountMenuStorageAdapter.AccountMenuGetById(
                    new AccountMenuGetByIdQuery { Id = query.Id.AsDefaultGuid() });
            if (accountMenuRModel == null)
            {
                response.SetFail("Account menu not existed");
                return;
            }

            response.Data = accountMenuRModel;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<AccountMenuRModel[]?>> SearchMenu
        (AccountMenuSearchQuery query)
    {
        return await CommonProcess<AccountMenuRModel[]?>(async (response) =>
        {
            var accountMenuRModels =
                await accountMenuStorageAdapter.AccountMenuSearch(query);
            if (accountMenuRModels == null || accountMenuRModels.Count == 0)
            {
                response.SetFail("Account menu list not existed");
                return;
            }

            response.Data = accountMenuRModels.ToArray();
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<AccountMenuRModel?>> GetByParentIdMenu(AccountMenuGetByParentIdQuery query)
    {
        return await CommonProcess<AccountMenuRModel?>(async (response) =>
        {
            var accountMenuRModel =
                await accountMenuStorageAdapter.AccountMenuGetByParentId(
                    new AccountMenuGetByParentIdQuery { ParentId = query.ParentId.AsDefaultGuid() });
            if (accountMenuRModel == null)
            {
                response.SetFail("Account menu not existed");
                return;
            }

            response.Data = accountMenuRModel;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<AccountMenuRModel[]?>> GetAllMenu()
    {
        return await CommonProcess<AccountMenuRModel[]?>(async (response) =>
        {
            var accountMenuRModels =
                await accountMenuStorageAdapter.AccountMenuGetAll();
            if (accountMenuRModels == null)
            {
                response.SetFail("Account menu list not existed");
                return;
            }

            response.Data = AccountMenuRModel.ToTree(accountMenuRModels);
            response.SetSuccess();
        });
    }
}