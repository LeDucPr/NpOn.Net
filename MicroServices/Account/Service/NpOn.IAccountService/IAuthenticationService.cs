using System.ServiceModel;
using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Infrastructures.NpOn.CommonDb.DbResults.Grpc;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Events;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

namespace MicroServices.Account.Service.NpOn.IAccountService;

[ServiceContract]
public interface IAuthenticationService
{
    [OperationContract]
    Task<CommonResponse<AccountLoginRModel>> Signup(AccountSignupCommand command);

    [OperationContract]
    Task<CommonResponse<bool>> ChangeAccountStatus(AccountSetStatusCommand command);

    [OperationContract]
    Task<CommonResponse<bool>> ChangeAccountPassword(AccountChangePasswordCommand command);

    [OperationContract]
    Task<CommonResponse<AccountLoginRModel>> Login(AccountLoginQuery query);

    [OperationContract]
    Task<CommonResponse<AccountLoginRModel>> RefreshToken(AccountRefreshTokenQuery query);

    [OperationContract]
    Task<CommonResponse<AccountLoginRModel>> GetLogonTokenBySessionId(AccountGetLogonInfoBySessionIdQuery query);

    [OperationContract]
    Task<CommonResponse<bool>> IsValidLogonPermissionExceptionControllers(
        AccountPermissionExceptionCachingCheckValidQuery query);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> LoginToken(CommonJsonQuery query);

    [OperationContract]
    Task<CommonResponse<string>> LogOut(AccountLogoutQuery query);

    Task<CommonResponse> SaveLogin(AccountSaveLoginEvent @event);

    Task<CommonResponse> SaveLogout(AccountSaveLogoutEvent @event);

    [OperationContract]
    Task<CommonResponse> AccountSyncFromOldSystem(AccountSyncFromOldSystemCommand[] commands);
}