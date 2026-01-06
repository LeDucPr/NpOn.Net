using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.BaseRepository.Postgres;
using Common.Infrastructures.NpOn.CommonDb.DbResults;
using Definitions.NpOn.ProjectConstant.AccountConstant;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;
using MicroServices.General.Contract.GeneralServiceContract.Queries;
using MicroServices.General.Service.NpOn.IGeneralService;

namespace MicroServices.Account.StorageAdapter.NpOn.AccountStorageAdapter;

public class AuthenticationStorageAdapter(
    IPostgresFactoryWrapper postgresFactoryWrapper,
    IFldMasterPgService fldMasterPgService
) : IAuthenticationStorageAdapter
{
    public async Task<List<AccountRModel>?> AccountGetByNumberPhoneOrEmailOrUsername(string phoneNumber,
        string email, string username)
    {
        var checkExistExecution = new TblFldExecution
        {
            Code = AuthenServiceQueryCode.AccountGetByUsernameOrPhoneNumberOrEmail,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "phone_number",
                    StringValue = phoneNumber
                },
                new TblFldExecutionParam
                {
                    ParamName = "email",
                    StringValue = email
                },
                new TblFldExecutionParam
                {
                    ParamName = "username",
                    StringValue = username
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(checkExistExecution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToList<AccountRModel>();
    }

    public async Task<AccountRModel?> AccountGetByUsernameAndPassword(string username, string password)
    {
        var execution = new TblFldExecution
        {
            Code = AuthenServiceQueryCode.AccountGetByUsernameAndPassword,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "username",
                    StringValue = username
                },
                new TblFldExecutionParam
                {
                    ParamName = "password",
                    StringValue = password
                }
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToFirstOrDefault<AccountRModel>();
    }

    public async Task<List<AccountLoginRModel>?> AccountLoginInfoGetByRefreshToken(string refreshToken)
    {
        var execution = new TblFldExecution
        {
            Code = AuthenServiceQueryCode.AccountLoginInfoGetByRefreshToken,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "refresh_token",
                    StringValue = refreshToken
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(execution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToList<AccountLoginRModel>();
    }

    public async Task<AccountRModel?> AccountGetById(string accountId) // Guid
    {
        var accountExecution = new TblFldExecution
        {
            Code = AuthenServiceQueryCode.AccountGetById,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "id",
                    StringValue = accountId,
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(accountExecution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToFirstOrDefault<AccountRModel>();
    }

    public async Task<List<AccountRModel>?> AccountGetByIds(string[]? accountIds) // Guids
    {
        var accountExecution = new TblFldExecution
        {
            Code = AuthenServiceQueryCode.AccountGetByIds,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "ids",
                    StringValue = accountIds.AsArrayJoin(),
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(accountExecution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToList<AccountRModel>();
    }
    public async Task<AccountLoginRModel?> AccountLoginInfoGetBySessionId(string sessionId)
    {
        var logoutExecution = new TblFldExecution
        {
            Code = AuthenServiceQueryCode.AccountLoginInfoGetBySessionId,
            ExecParams =
            [
                new TblFldExecutionParam
                {
                    ParamName = "session_id",
                    StringValue = sessionId
                },
            ]
        };
        var commandResponse = await fldMasterPgService.GetExecCommand(logoutExecution);
        if (!commandResponse.Status || commandResponse.Data == null)
            return null;
        var result = await postgresFactoryWrapper.Execute(commandResponse.Data.ToCommand());
        return result.ToFirstOrDefault<AccountLoginRModel>();
    }
}