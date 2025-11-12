using System.ServiceModel;
using CommonDb.DbResults.Grpc;
using CommonObject.CommonObjects;

namespace IFaCareTestService;

[ServiceContract]
public interface ITestQueueService
{
    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> TestQueue2C();
}