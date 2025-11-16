using System.ServiceModel;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;

namespace IAccountService;

[ServiceContract]
public interface IAuthenticationService
{
    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> Login();
}
