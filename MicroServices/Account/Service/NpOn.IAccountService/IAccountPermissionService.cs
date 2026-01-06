using System.ServiceModel;
using Common.Extensions.NpOn.CommonGrpcContract;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

namespace MicroServices.Account.Service.NpOn.IAccountService;

[ServiceContract]
public interface IAccountPermissionService
{
    [OperationContract]
    Task<CommonResponse<AccountPermissionExceptionRModel[]?>> AccountPermissionExceptionQuickGetByAccountId(
        AccountPermissionExceptionGetByAccountIdQuery exceptionGetByAccountIdQuery);

    [OperationContract]
    Task<CommonResponse<bool>> SyncPermissionWithController(AccountPermissionControllerAddOrChangeCommand[] command);

    [OperationContract]
    Task<CommonResponse<bool>> ClearOldControllersVersion(
        AccountPermissionControllerDeleteByHostCodeAndVersionIdCommand command);

    [OperationContract]
    Task<CommonResponse<bool>> AddOrChangeAccountPermissionException(
        AccountPermissionExceptionAddOrChangeCommand[] commands);

    [OperationContract]
    Task<CommonResponse<bool>> AddOrChangeManyAccountPermissionException(
        AccountPermissionExceptionAddOrChangeManyCommand command);
}