using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.BaseRepository.Postgres;
using Common.Infrastructures.NpOn.CommonDb.DbResults;
using Definitions.NpOn.ProjectConstant.AccountConstant;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;
using MicroServices.General.Contract.GeneralServiceContract.Queries;
using MicroServices.General.Service.NpOn.IGeneralService;

namespace MicroServices.Account.StorageAdapter.NpOn.AccountStorageAdapter;

public class AccountInfoStorageAdapter(
    IPostgresFactoryWrapper postgresFactoryWrapper,
    IFldMasterPgService fldMasterPgService
) : IAccountInfoStorageAdapter
{
    // info
    public async Task<AccountInfoRModel?> AccountInfoActiveGetByAccountId(string accountId)
    {
        var execution = new TblFldExecution
        {
            Code = AccountServiceQueryCode.AccountInfoActiveGetByAccountId,
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
        return result.ToFirstOrDefault<AccountInfoRModel>();
    }

    public async Task<List<AccountInfoRModel>?> AccountInfoActiveGetByAccountIds(string[] accountIds)
    {
        var execution = new TblFldExecution
        {
            Code = AccountServiceQueryCode.AccountInfoActiveGetByAccountIds,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "account_ids",
                    StringValue = accountIds.AsArrayJoin()
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToList<AccountInfoRModel>();
    }


    // address
    public async Task<List<AccountAddressRModel>?> AccountAddressesGetByIds(string[] accountIds)
    {
        var execution = new TblFldExecution
        {
            Code = AccountServiceQueryCode.AccountAddressesGetByIds,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "ids",
                    StringValue = accountIds.AsArrayJoin()
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToList<AccountAddressRModel>();
    }

    public async Task<List<AccountAddressRModel>?> AccountAddressesDefaultGetByAccountIds(string[] accountIds)
    {
        var execution = new TblFldExecution
        {
            Code = AccountServiceQueryCode.AccountAddressesDefaultGetByAccountIds,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "account_ids",
                    StringValue = accountIds.AsArrayJoin()
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToList<AccountAddressRModel>();
    }
}