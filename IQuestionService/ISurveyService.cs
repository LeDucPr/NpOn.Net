using CommonGrpcObject;
using QuestionServiceObject.BusinessObjects;
using System.ServiceModel;
using QuestionServiceObject.QueryObjects;

namespace IQuestionService;

[ServiceContract]
public interface ISurveyService
{
    /// Lấy danh sách tất cả surveys
    /// </summary>
    [OperationContract]
    Task<CommonResponse<List<QuesSrvDiseaseObject>>> GetAllSurveys();

    // /// <summary>
    // /// Lấy thông tin chi tiết survey theo ID
    // /// </summary>
    // [OperationContract]
    // Task<CommonResponse<QuesSrvDiseaseObject>> GetSurveyById(Guid surveyId);

    // /// <summary>
    // /// Lấy survey với đầy đủ questions và options
    // /// </summary>
    // [OperationContract]
    // Task<CommonResponse<QuesSrvDiseaseFullObject>> GetSurveyWithQuestions(Guid surveyId);

    /// <summary>
    /// Lấy danh sách questions của survey
    /// </summary>
    [OperationContract]
    Task<CommonResponse<List<QuestionObject>>> GetQuestionsBySurvey(SurveyGetAllQuery query);

    // /// <summary>
    // /// Lấy question với answer options
    // /// </summary>
    // [OperationContract]
    // Task<CommonResponse<QuestionWithOptionsObject>> GetQuestionWithOptions(Guid questionId);

}
    