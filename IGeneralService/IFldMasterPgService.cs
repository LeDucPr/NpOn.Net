using System.ServiceModel;
using CommonGrpcObject;
using GeneralServiceObject.BusinessObjects;
using GeneralServiceObject.QueryObjects;

namespace IGeneralService;

[ServiceContract]
public interface IFldMasterPgService
{
    [OperationContract]
    Task<CommonResponse<List<TblFldObject>>> GetQuery(TblFldQuery query);
}