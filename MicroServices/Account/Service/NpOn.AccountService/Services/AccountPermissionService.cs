using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Common.Infrastructures.NpOn.BaseRepository.Postgres;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using MicroServices.Account.Service.NpOn.IAccountService;
using MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;

namespace MicroServices.Account.Service.NpOn.AccountService.Services;

public class AccountPermissionService(
    IAuthenticationStorageAdapter authenticationStorageAdapter,
    IAccountPermissionStorageAdapter accountPermissionStorageAdapter,
    IAccountTokenAndPermissionRedisRepository redisRepository,
    IAccountGroupStorageAdapter accountGroupStorageAdapter,
    IPostgresBaseRepository baseRepository,
    ILogger<AccountPermissionService> logger
) : CommonService(logger), IAccountPermissionService
{
    public async Task<CommonResponse<AccountPermissionExceptionRModel[]?>>
        AccountPermissionExceptionQuickGetByAccountId(
            AccountPermissionExceptionGetByAccountIdQuery exceptionGetByAccountIdQuery)
    {
        return await CommonProcess<AccountPermissionExceptionRModel[]?>(async (response) =>
        {
            List<AccountPermissionExceptionRModel>? accountPermissionException =
                await accountPermissionStorageAdapter.AccountPermissionExceptionGetByAccountId(
                    exceptionGetByAccountIdQuery.AccountId);
            response.Data = accountPermissionException?.ToArray();
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<bool>> SyncPermissionWithController(
        AccountPermissionControllerAddOrChangeCommand[] command)
    {
        return await CommonProcess<bool>(async (response) =>
        {
            List<AccountPermissionController> accountPermissions =
                command.Select(x => new AccountPermissionController(x))
                    .ToList();
            if (!(await baseRepository.Merge(accountPermissions))?.Status ?? false)
            {
                response.SetFail("Sync AccountPermissionControllers fail");
                return;
            }

            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<bool>> ClearOldControllersVersion(
        AccountPermissionControllerDeleteByHostCodeAndVersionIdCommand command)
    {
        return await CommonProcess<bool>(async (response) =>
        {
            if (!await accountPermissionStorageAdapter.AccountPermissionExceptionDeleteOldVersionByHostCode(
                    command.HostCode, command.VersionId.AsDefaultString()))
            {
                response.SetFail("ClearOldControllersVersion fail");
                return;
            }

            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<bool>> AddOrChangeAccountPermissionException(
        AccountPermissionExceptionAddOrChangeCommand[] commands)
    {
        return await CommonProcess<bool>(async (response) =>
        {
            if (commands.Length == 0)
            {
                response.SetSuccess();
                return;
            }

            if (commands.Select(x => x.AccountId).Distinct().ToArray() is { Length: > 1 })
            {
                response.SetFail("To many AccountId in Commands");
                return;
            }

            if (commands.Select(x => x.ControllerCode).Distinct().ToArray().Length != commands.Length)
            {
                response.SetFail("ControllerCode Duplicated in Commands");
                return;
            }

            string accountIdAsString = commands.First().AccountId.AsDefaultString();
            AccountPermissionException[] permissionExceptions =
                commands.Select(AccountPermissionException.FromCommands).ToArray();

            // check permission of account before update into AccountPermissionException table
            AccountRModel? accountTakePermission = await authenticationStorageAdapter.AccountGetById(accountIdAsString);
            EPermission[]? accountPermission = accountTakePermission?.Permissions;
            if (accountTakePermission == null || accountPermission is not { Length: > 0 })
            {
                response.SetFail("Account not found");
                return;
            }

            // existed exceptions 
            List<AccountPermissionControllerRModel>? rAccountPermissions =
                await accountPermissionStorageAdapter.AccountPermissionControllerGetByCodes(permissionExceptions
                    .Select(x => x.ControllerCode).ToArray());

            if (rAccountPermissions is not { Count: > 0 } ||
                rAccountPermissions.Count != permissionExceptions.Length)
            {
                response.SetFail("ControllerCode not found");
                return;
            }

            string[] basePermissionCodes =
                rAccountPermissions.Where(x => accountPermission.Contains(x.Permission))
                    .Select(x => x.Code).ToArray();
            string[] exceptPermissionCodes =
                rAccountPermissions.Select(x => x.Code).Except(basePermissionCodes).ToArray();
            // discard endpoints have basic permission (disable need to Change, enable => ignore (delete))
            List<AccountPermissionException> permissionExceptionsNeedChange =
                permissionExceptions.Where(x =>
                    (basePermissionCodes.Contains(x.ControllerCode) // contain permission and want to disable
                     && x.AccessPermission == EPermissionAccessController.Disable)
                    ||
                    (exceptPermissionCodes.Contains(x.ControllerCode) // not contain permission and want to enable
                     && x.AccessPermission == EPermissionAccessController.Enable)
                ).ToList();

            List<AccountPermissionException> permissionExceptionsNeedDelete = permissionExceptions
                .Where(x => permissionExceptionsNeedChange.All(c => c.ControllerCode != x.ControllerCode)).ToList();
            if (permissionExceptionsNeedChange is { Count: > 0 })
                if (!(await baseRepository.Merge(permissionExceptionsNeedChange))?.Status ?? false)
                {
                    response.SetFail("Add/Change AccountPermissionExceptions fail");
                    return;
                }

            if (permissionExceptionsNeedDelete is { Count: > 0 })
                if (!(await baseRepository.Delete(permissionExceptionsNeedDelete))?.Status ?? false)
                {
                    response.SetFail("Delete AccountPermissionExceptions fail");
                    return;
                }

            var isDeleteCachingTokenStorage =
                await redisRepository.DeleteCachingTokenStorageAndTokensByAccountId(accountIdAsString);
            var isDeleteCachePermissionCache =
                await redisRepository.DeleteCachingPermissionExceptionsByAccountId(accountIdAsString);
            if (!isDeleteCachingTokenStorage || !isDeleteCachePermissionCache)
            {
                response.SetFail("Delete Token when change Permission fail");
                return;
            }

            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<bool>> AddOrChangeManyAccountPermissionException(
        AccountPermissionExceptionAddOrChangeManyCommand command)
    {
        return await CommonProcess<bool>(async (response) =>
        {
            if (command.AccountIds is not { Length: > 0 } && command.GroupIds is not { Length: > 0 })
            {
                response.SetFail("AccountIds and GroupIds not null");
                return;
            }

            string[]? controllerCodes = command.ControllerComponents?
                .Select(x => x.ControllerCode).ToArray();
            List<AccountPermissionControllerRModel>? rAccountPermissions =
                await accountPermissionStorageAdapter.AccountPermissionControllerGetByCodes(controllerCodes);

            if (rAccountPermissions is not { Count: > 0 } ||
                rAccountPermissions.Count != controllerCodes?.Length)
            {
                response.SetFail("ControllerCode not found");
                return;
            }

            List<Guid> accountIds = [];
            if (command.AccountIds is { Length: > 0 })
                accountIds.AddRange(command.AccountIds.Distinct());


            string[]? groupIdAsStrings = command.GroupIds?.Select(x => x.AsDefaultString()).ToArray();
            int batchSize = 200;
            int pageIndex = 0;
            while (true)
            {
                if (accountIds is { Count : > 0 })
                {
                    var permissionExceptions = accountIds
                        .SelectMany(accountId => command.ControllerComponents!.Select(controllerComponent =>
                            new AccountPermissionException
                            {
                                AccountId = accountId,
                                ControllerCode = controllerComponent.ControllerCode,
                                AccessPermission = controllerComponent.AccessPermission,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            }))
                        .ToArray();

                    if (!await ChangePermissionAccount(
                            accountIds.ToArray(), permissionExceptions, rAccountPermissions))
                        break;
                    pageIndex++;
                }

                if (groupIdAsStrings == null)
                    break;
                var accountGroups =
                    await accountGroupStorageAdapter.AccountGroupGetByGroupIds(
                        groupIdAsStrings, batchSize, pageIndex);
                if (accountGroups is not { Count : > 0 })
                    break;
                accountIds = accountGroups.Where(x => x.Member != Guid.Empty)
                    .Select(x => x.Member.AsDefaultGuid()).Distinct().ToList();
            }

            response.SetSuccess();
        });
    }

    private async Task<bool> ChangePermissionAccount(Guid[]? accountIds,
        AccountPermissionException[] permissionExceptions,
        List<AccountPermissionControllerRModel> rAccountPermissions)
    {
        if (accountIds is not { Length: > 0 })
            return true;
        List<AccountRModel>? accountsTakePermission =
            await authenticationStorageAdapter.AccountGetByIds(accountIds.Select(x => x.ToString()).ToArray());
        if (accountsTakePermission is not { Count: > 0 })
            return true;

        List<string> accountIdAsStringNeedLogouts = [];
        List<AccountPermissionException> permissionExceptionsNeedChange = [];
        List<AccountPermissionException> permissionExceptionsNeedDelete = [];
        foreach (AccountRModel accountTakePermission in accountsTakePermission)
        {
            EPermission[]? accountPermission = accountTakePermission.Permissions;
            if (accountPermission is not { Length: > 0 })
                continue;
            string[] basePermissionCodes =
                rAccountPermissions.Where(x => accountPermission.Contains(x.Permission))
                    .Select(x => x.Code).ToArray();
            string[] exceptPermissionCodes =
                rAccountPermissions.Select(x => x.Code).Except(basePermissionCodes).ToArray();

            // Add Change Actions
            permissionExceptionsNeedChange.AddRange(
                permissionExceptions.Where(x =>
                    (basePermissionCodes.Contains(x.ControllerCode) // contain permission and want to disable
                     && x.AccessPermission == EPermissionAccessController.Disable)
                    ||
                    (exceptPermissionCodes.Contains(x.ControllerCode) // not contain permission and want to enable
                     && x.AccessPermission == EPermissionAccessController.Enable)
                )
            );
            permissionExceptionsNeedDelete.AddRange(
                permissionExceptions.Where(x =>
                    (basePermissionCodes.Contains(x.ControllerCode)
                     && x.AccessPermission == EPermissionAccessController.Enable)
                    ||
                    (exceptPermissionCodes.Contains(x.ControllerCode)
                     && x.AccessPermission == EPermissionAccessController.Disable)
                )
            );
            accountIdAsStringNeedLogouts.Add(accountTakePermission.Id.AsDefaultString());
        }

        if (permissionExceptionsNeedChange is { Count: > 0 })
            if (!(await baseRepository.Merge(permissionExceptionsNeedChange))?.Status ?? false)
                return false;

        if (permissionExceptionsNeedDelete is { Count: > 0 })
            if (!(await baseRepository.Delete(permissionExceptionsNeedDelete))?.Status ?? false)
                return false;

        var isDeleteCachingTokenStorage =
            await redisRepository.DeleteCachingTokenStorageAndTokensByAccountIds(accountIdAsStringNeedLogouts
                .ToArray());
        var isDeleteCachePermissionCache =
            await redisRepository.DeleteCachingPermissionExceptionsByAccountIds(accountIdAsStringNeedLogouts.ToArray());
        return isDeleteCachingTokenStorage && isDeleteCachePermissionCache;
    }
}