using System.ServiceModel;
using CommonDb.DbResults.Grpc;
using CommonObject.CommonObjects;
using ProtoBuf.Grpc;

namespace IFaCareTestService;

[ServiceContract]
public interface IFaCareTService
{
    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> TestCallSgnR(CallContext context  = default);
}