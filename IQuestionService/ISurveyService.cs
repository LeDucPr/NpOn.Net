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
    Task<CommonResponse<INpOnGrpcObject>> CalculateScore(CalculateSurveyScoreQuery query);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> GetSurveyOutcomes(SurveyOutcomeScoreQuery query);
    
    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> GetSurveyHistory(SurveyHistoryQuery query);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> GetAnswersScore(AnswersScoreQuery query);

    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> GetMaxSurveyScore(MaxSurveyScoreQuery query);
}