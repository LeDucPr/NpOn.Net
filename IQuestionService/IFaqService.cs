using System.ServiceModel;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using QuestionServiceObject;
using QuestionServiceObject.BusinessObjects;
using QuestionServiceObject.QueryObjects;


namespace IQuestionService;

[ServiceContract]
public interface IFaqService
{
    [OperationContract]
    Task<CommonResponse<FaqObject>> GetAll(FaqQuery query);
}