using System.ServiceModel;
using Common.Extensions.NpOn.CommonGrpcContract;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;

namespace MicroServices.Account.Service.NpOn.IAccountService;

[ServiceContract]
public interface IAccountGroupService
{
    [OperationContract]
    Task<CommonResponse> GroupAddOrChange(AccountGroupAddOrChangeCommand command);

    [OperationContract]
    Task<CommonResponse> GroupCopy(AccountGroupCopyCommand command);

    [OperationContract]
    Task<CommonResponse> Search(AccountGroupSearchQuery query);

    [OperationContract]
    Task<CommonResponse> GroupOrMemberDelete(AccountGroupDeleteCommand command);
}