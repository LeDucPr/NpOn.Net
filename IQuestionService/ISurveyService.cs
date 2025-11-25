using CommonGrpcObject;
using System.ServiceModel;
using CommonDb.DbResults.Grpc;
using QuestionServiceObject.CommandObjects;
using QuestionServiceObject.QueryObjects;

namespace IQuestionService;

[ServiceContract]
public interface ISurveyService
{
    [OperationContract]
    Task<CommonResponse<string>> AddOrUpdateSurvey(SurveyAddOrUpdateCommand addOrUpdateCommand);
    
    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> GetQuestionsBySurveyId(QuestionGetBySurveyIdQuery query);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> GetQuestionsByUserIdAndSurveyId(QuestionGetByUserIdAndSurveyIdQuery query);
    
    [OperationContract]
    Task<CommonResponse<string>> SubmitAnswers(SubmitSurveyCommand command);
    
    [OperationContract]
    Task<CommonResponse<int>> CalculateScore(CalculateSurveyScoreQuery query);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> GetSurveyOutcomes(string surveyId);
}