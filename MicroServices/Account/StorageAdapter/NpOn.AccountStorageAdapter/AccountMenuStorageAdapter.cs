using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.BaseRepository.Postgres;
using Common.Infrastructures.NpOn.CommonDb.DbResults;
using Definitions.NpOn.ProjectConstant.AccountConstant;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;
using MicroServices.General.Contract.GeneralServiceContract.Queries;
using MicroServices.General.Service.NpOn.IGeneralService;

namespace MicroServices.Account.StorageAdapter.NpOn.AccountStorageAdapter;

public class AccountMenuStorageAdapter(
    IPostgresFactoryWrapper postgresFactoryWrapper,
    IFldMasterPgService fldMasterPgService) : IAccountMenuStorageAdapter
{
    public async Task<AccountMenuRModel?> AccountMenuGetById(AccountMenuGetByIdQuery query)
    {
        var execution = new TblFldExecution
        {
            Code = AccountMenuServiceQueryCode.AccountMenuGetById,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "id",
                    StringValue = query.Id.AsDefaultString()
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToFirstOrDefault<AccountMenuRModel>();
    }

    public async Task<AccountMenuRModel?> AccountMenuGetByParentId(AccountMenuGetByParentIdQuery query)
    {
        var execution = new TblFldExecution
        {
            Code = AccountMenuServiceQueryCode.AccountMenuGetByParentId,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "parent_id",
                    StringValue = query.ParentId.AsDefaultString()
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToFirstOrDefault<AccountMenuRModel>();
    }

    public async Task<List<AccountMenuRModel>?> AccountMenuSearch(AccountMenuSearchQuery query)
    {
        var execution = new TblFldExecution
        {
            Code = AccountMenuServiceQueryCode.AccountMenuSearch,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "keyword",
                    StringValue = query.Keyword
                },
                new TblFldExecutionParam
                {
                    ParamName = "page_size",
                    StringValue = query.PageSize.AsDefaultString()
                },
                new TblFldExecutionParam
                {
                    ParamName = "page_index",
                    StringValue = query.PageIndex.AsDefaultString()
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToList<AccountMenuRModel>();
    }

    public async Task<List<AccountMenuRModel>?> AccountMenuGetAll()
    {
        var execution = new TblFldExecution
        {
            Code = AccountMenuServiceQueryCode.AccountMenuGetAll,
            // ExecParams =
            // [
            // ]
        };

        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToList<AccountMenuRModel>();
    }
}