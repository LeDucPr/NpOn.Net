using System.ServiceModel;
using CommonDb.DbResults.Grpc;
using CommonObject.CommonObjects;

namespace ITZoneService;

[ServiceContract]
public interface ICfService
{
    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> TestC();
}