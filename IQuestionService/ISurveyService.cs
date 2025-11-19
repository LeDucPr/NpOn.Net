using CommonGrpcObject;
using QuestionServiceObject.BusinessObjects;
using System.ServiceModel;

namespace IQuestionService;

[ServiceContract]
public interface ISurveyService
{
    /// Lấy danh sách tất cả surveys
    /// </summary>
    [OperationContract]
    Task<CommonResponse<List<SurveysObject>>> GetAllSurveys();

    /// <summary>
    /// Lấy thông tin chi tiết survey theo ID
    /// </summary>
    [OperationContract]
    Task<CommonResponse<SurveysObject>> GetSurveyById(Guid surveyId);

    /// <summary>
    /// Lấy survey với đầy đủ questions và options
    /// </summary>
    [OperationContract]
    Task<CommonResponse<SurveyFullObject>> GetSurveyWithQuestions(Guid surveyId);

    /// <summary>
    /// Lấy danh sách questions của survey
    /// </summary>
    [OperationContract]
    Task<CommonResponse<List<QuestionObject>>> GetQuestionsBySurvey(Guid surveyId);

    /// <summary>
    /// Lấy question với answer options
    /// </summary>
    [OperationContract]
    Task<CommonResponse<QuestionWithOptionsObject>> GetQuestionWithOptions(Guid questionId);

}
    