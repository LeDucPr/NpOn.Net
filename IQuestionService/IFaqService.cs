using System.ServiceModel;
using CommonGrpcObject;
using QuestionServiceObject.BusinessObjects;
using QuestionServiceObject.QueryObjects;

namespace IQuestionService;

[ServiceContract]
public interface IFaqService
{
    [OperationContract]
    Task<CommonResponse<FaqObject>> Login(FaqQuery query);
}