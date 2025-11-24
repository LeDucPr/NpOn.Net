using System.ServiceModel;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using GeneralServiceObject.BusinessObjects;
using GeneralServiceObject.QueryObjects;

namespace IGeneralService;

[ServiceContract]
public interface IFldMasterPgService
{
    [OperationContract]
    Task<CommonResponse<List<TblFldObject>>> GetExecution(TblFldExecution execution);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> Execute(TblFldExecution execution);
}