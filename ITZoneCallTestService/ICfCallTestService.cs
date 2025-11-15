using System.ServiceModel;
using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonObject.CommonObjects;

namespace ITZoneCallTestService;

[ServiceContract]
public interface ICfCallTestService
{
    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> TestCallC();
}