using System.ServiceModel;
using AccountServiceObject.BusinessObjects;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;

namespace IAccountService;

[ServiceContract]
public interface IUserService
{
    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> GetAccountInfoAsGenericTable();
    
    [OperationContract]
    Task<CommonResponse<AccountInfoAliasTestObject>> GetAccountInfo();
}