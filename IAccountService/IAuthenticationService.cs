using System.ServiceModel;
using AccountServiceObject.BusinessObjects;
using AccountServiceObject.QueryObjects;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;

namespace IAccountService;

[ServiceContract]
public interface IAuthenticationService
{
    [OperationContract]
    Task<CommonResponse<AccountLoginInfoObject>> Login(AccountLoginQuery query);
    
    [OperationContract]
    Task<CommonResponse<AccountInfoAliasTestObject>> LoginJ(CommonJsonQuery query);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> RefreshToken(CommonJsonQuery query);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> LoginToken(CommonJsonQuery query);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> Info();

    [OperationContract]
    Task<CommonResponse> LogOut();
}