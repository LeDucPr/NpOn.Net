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
    Task<CommonResponse<List<TblFldObject>>> GetQuery(TblFldQuery query);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> Query(TblFldQuery query);
}