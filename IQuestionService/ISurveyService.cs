using CommonGrpcObject;
using QuestionServiceObject.BusinessObjects;
using System.ServiceModel;
using CommonDb.DbResults.Grpc;
using QuestionServiceObject.QueryObjects;

namespace IQuestionService;

[ServiceContract]
public interface ISurveyService
{
    /// <summary>
    /// Lấy danh sách tất cả surveys
    /// </summary>
    [OperationContract]
    Task<CommonResponse<List<QuesSrvDiseaseObject>>> GetAllSurveys();

    
    /// <summary>
    /// Lấy danh sách questions của survey
    /// </summary>
    [OperationContract]
    Task<CommonResponse<INpOnGrpcObject>> GetQuestionsBySurveyId(SurveyGetAllQuery query);
    
    /// <summary>
    /// Lấy danh sách questions của survey
    /// </summary>
    [OperationContract]
    Task<CommonResponse<List<QuestionObject>>> GetQuestionsBySurvey(SurveyGetAllQuery query);
}