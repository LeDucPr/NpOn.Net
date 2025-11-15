using System.ServiceModel;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonObject.CommonObjects;
using FaCareTestServiceObject.ServiceObjects.RabbitMqEvents;

namespace IFaCareTestService;

[ServiceContract]
public interface ITestQueueService
{
    [OperationContract]
    Task<CommonResponse<string>> TestRabbitMqHandler();
    
    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> TestQueue2C();
    
    [OperationContract]
    Task<CommonResponse<string>> ProcessEventRbMqT2(RabbitMqTestEvent @event);
}