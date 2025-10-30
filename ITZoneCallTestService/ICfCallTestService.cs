using System.ServiceModel;
using CommonDb.DbResults;
using CommonObject.CommonObjects;

namespace ITZoneCallTestService;

[ServiceContract]
public interface ICfCallTestService
{
    [OperationContract]
    Task<CommonResponse<INpOnWrapperResult>> TestCallC();
}