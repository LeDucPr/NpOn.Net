using System.ServiceModel;
using Common.Extensions.NpOn.CommonGrpcContract;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

namespace MicroServices.Account.Service.NpOn.IAccountService;

[ServiceContract]
public interface IAccountMenuService
{
    [OperationContract]
    Task<CommonResponse> AddOrChangeAccountMenu(AccountMenuAddOrChangeCommand command);

    [OperationContract]
    Task<CommonResponse> DeleteAccountMenu(AccountMenuDeleteCommand command);

    [OperationContract]
    Task<CommonResponse<AccountMenuRModel?>> GetByIdMenu(AccountMenuGetByIdQuery query);

    [OperationContract]
    Task<CommonResponse<AccountMenuRModel[]?>> SearchMenu(AccountMenuSearchQuery query);

    [OperationContract]
    Task<CommonResponse<AccountMenuRModel?>> GetByParentIdMenu(AccountMenuGetByParentIdQuery query);

    [OperationContract]
    Task<CommonResponse<AccountMenuRModel[]?>> GetAllMenu();
}