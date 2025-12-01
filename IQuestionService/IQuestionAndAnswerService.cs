using System.ServiceModel;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using QuestionServiceObject.CommandObjects;

namespace IQuestionService;

[ServiceContract]
public interface IQuestionAndAnswerService
{
    [OperationContract]
    Task<CommonResponse<string>> InsertUserAnswer(List<UserAnswerSubmitCommand> commands);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> InsertUserResult(SurveyResultInsertCommand command);
}