using System.ServiceModel;
using CommonGrpcObject;
using QuestionServiceObject.CommandObjects;

namespace IQuestionService;

[ServiceContract]
public interface IQuestionAndAnswerService
{
    [OperationContract]
    Task<CommonResponse<string>> SubmitAnswers(SubmitSurveyCommand command);
}