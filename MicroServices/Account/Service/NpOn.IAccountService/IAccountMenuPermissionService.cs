using System.ServiceModel;

namespace MicroServices.Account.Service.NpOn.IAccountService;

[ServiceContract]
public interface IAccountMenuPermissionService
{
    // [OperationContract]
    // Task<CommonResponse<AccountInfoRModel[]?>> AccountInfoGetByAccountIds(AccountInfoGetByAccountIdsQuery query);
}