using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.BaseRepository.Postgres;
using Common.Infrastructures.NpOn.CommonDb.DbResults;
using Definitions.NpOn.ProjectConstant.AccountConstant;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;
using MicroServices.General.Contract.GeneralServiceContract.Queries;
using MicroServices.General.Service.NpOn.IGeneralService;

namespace MicroServices.Account.StorageAdapter.NpOn.AccountStorageAdapter;

public class AccountGroupStorageAdapter(
    IPostgresFactoryWrapper postgresFactoryWrapper,
    IFldMasterPgService fldMasterPgService
) : IAccountGroupStorageAdapter
{
    public async Task<List<AccountGroupRModel>?> AccountGroupGetByGroupIds(
        string[] groupIds, int pageSize, int pageIndex) // Guids
    {
        var execution = new TblFldExecution
        {
            Code = AccountGroupServiceCode.AccountGroupGetByGroupIds,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "group_ids",
                    StringValue = groupIds.AsArrayJoin()
                },
                new TblFldExecutionParam
                {
                    ParamName = "page_size",
                    StringValue = pageSize.AsDefaultString()
                },
                new TblFldExecutionParam
                {
                    ParamName = "page_index",
                    StringValue = pageIndex.AsDefaultString()
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToList<AccountGroupRModel>();
    }
}