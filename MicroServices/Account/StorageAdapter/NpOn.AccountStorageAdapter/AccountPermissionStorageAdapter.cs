using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.BaseRepository.Postgres;
using Common.Infrastructures.NpOn.CommonDb.DbResults;
using Definitions.NpOn.ProjectConstant.AccountConstant;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;
using MicroServices.General.Contract.GeneralServiceContract.Queries;
using MicroServices.General.Service.NpOn.IGeneralService;

namespace MicroServices.Account.StorageAdapter.NpOn.AccountStorageAdapter;

public class AccountPermissionStorageAdapter(
    IPostgresFactoryWrapper postgresFactoryWrapper,
    IFldMasterPgService fldMasterPgService
) : IAccountPermissionStorageAdapter
{
    public async Task<List<AccountPermissionExceptionRModel>?> AccountPermissionExceptionGetByAccountId(
        string accountId)
    {
        var execution = new TblFldExecution
        {
            Code = AccountPermissionServiceQueryCode.AccountExceptionControllersGetByAccountId,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "account_id",
                    StringValue = accountId
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToList<AccountPermissionExceptionRModel>();
    }

    public async Task<bool> AccountPermissionExceptionDeleteOldVersionByHostCode(string hostCode, string versionId)
    {
        var execution = new TblFldExecution
        {
            Code = AccountPermissionServiceQueryCode.AccountExceptionControllersDeleteOldVersionByHostCode,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "host_code",
                    StringValue = hostCode
                },
                new TblFldExecutionParam
                {
                    ParamName = "version_id",
                    StringValue = versionId
                }
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return false;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result?.Status ?? false;
    }

    public async Task<List<AccountPermissionExceptionRModel>?>
        AccountPermissionExceptionGetByAccountIdAndControllerCodes(
            string accountId, string[] controllerCodes)
    {
        var execution = new TblFldExecution
        {
            Code = AccountPermissionServiceQueryCode.AccountExceptionControllersGetByAccountIdAndControllerCodes,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "account_id",
                    StringValue = accountId
                },
                new TblFldExecutionParam
                {
                    ParamName = "controller_codes",
                    StringValue = controllerCodes.AsArrayJoin()
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToList<AccountPermissionExceptionRModel>();
    }

    public async Task<List<AccountPermissionControllerRModel>?> AccountPermissionControllerGetByCodes(string[]? codes)
    {
        if (codes is not { Length: > 0 })
            return null;
        var execution = new TblFldExecution
        {
            Code = AccountPermissionServiceQueryCode.AccountPermissionControllerGetByCodes,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "codes",
                    StringValue = codes.AsArrayJoin()
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToList<AccountPermissionControllerRModel>();
    }
}