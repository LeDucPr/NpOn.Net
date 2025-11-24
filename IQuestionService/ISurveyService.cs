using CommonGrpcObject;
using System.ServiceModel;
using CommonDb.DbResults.Grpc;
using QuestionServiceObject.QueryObjects;

namespace IQuestionService;

[ServiceContract]
public interface ISurveyService
{
    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> GetQuestionsBySurveyId(QuestionGetBySurveyIdQuery query);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> GetQuestionsByUserIdAndSurveyId(QuestionGetByUserIdAndSurveyIdQuery query);
}