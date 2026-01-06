using System.ServiceModel;
using Common.Extensions.NpOn.CommonGrpcContract;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

namespace MicroServices.Account.Service.NpOn.IAccountService;

[ServiceContract]
public interface IAccountInfoService
{
    [OperationContract]
    Task<CommonResponse<AccountInfoRModel[]?>> AccountInfoGetByAccountIds(AccountInfoGetByAccountIdsQuery query);

    [OperationContract]
    Task<CommonResponse<AccountInfoRModel?>> AccountInfoGetByAccountId(AccountInfoGetByAccountIdQuery query);

    [OperationContract]
    Task<CommonResponse> AccountInfoAddOrChange(AccountInfoAddOrChangeCommand command);

    [OperationContract]
    Task<CommonResponse> AccountAddressesAddOrChange(AccountAddressAddOrChangeCommand[] commands);
}