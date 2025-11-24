using CommonDb.DbCommands;
using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonWebApplication.Services;
using DbFactory;
using GeneralServiceObject.QueryObjects;
using HandleFlow.ResultConverters;
using IGeneralService;
using IQuestionService;
using NpgsqlTypes;
using QuestionServiceObject.BusinessObjects;
using QuestionServiceObject.QueryObjects;

namespace QuestionService.Services;

public class SurveyService(
    IDbFactoryWrapper dbFactoryWrapper,
    IFldMasterPgService fldMasterPgService,
    ILogger<CommonService> logger
) : CommonService(logger), ISurveyService
{
    /// <summary>
    /// Lấy danh sách tất cả surveys
    /// </summary>
    public async Task<CommonResponse<List<QuesSrvDiseaseObject>>> GetAllSurveys()
    {
        return await CommonProcess<List<QuesSrvDiseaseObject>>(async (response) =>
        {
            string pgQuery = @"
                    SELECT 
                        id,
                        title,
                        description,
                        max_total_score,
                        is_published,
                        created_at,
                        updated_at
                    FROM surveys
                    WHERE is_published = true
                    ORDER BY created_at DESC";

            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.QueryAsync(pgQuery);

            List<QuesSrvDiseaseObject>? surveyObjects = resultOfQuery?
                .GenericConverter(typeof(QuesSrvDiseaseObject))?
                .Cast<QuesSrvDiseaseObject>()
                .ToList();

            if (surveyObjects is not { Count: > 0 })
            {
                response.SetFail("Không tìm thấy survey nào");
                return;
            }

            response.Data = surveyObjects;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<INpOnGrpcObject>> GetQuestionsBySurveyId(SurveyGetAllQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var surveyGetBy = await fldMasterPgService.Query(new TblFldQuery()
            {
                Code = "questions_by_survey_id",
                QueryParams =
                [
                    new TblFldQueryParam()
                    {
                        ParamName = "survey_id",
                        StringValue = query.SurveyIdAsString
                    }
                ],
            });

            INpOnGrpcObject? questionGrpTable = surveyGetBy.Data;
            if (!surveyGetBy.Status)
            {
                response.SetFail(surveyGetBy.ErrorMessages);
                return;
            }

            response.Data = questionGrpTable;
            response.SetSuccess();
        });
    }

    /// <summary>
    /// Lấy danh sách questions của survey
    /// </summary>
    public async Task<CommonResponse<List<QuestionObject>>> GetQuestionsBySurvey(SurveyGetAllQuery query)
    {
        return await CommonProcess<List<QuestionObject>>(async (response) =>
        {
            string queryString = @"
                    SELECT * FROM ques_srv_question
                        WHERE ques_srv_survey_id = @ques_srv_survey_id
                        ORDER BY question_order
                    ";

            NpOnDbCommandParam param = new NpOnDbCommandParam<NpgsqlDbType>
            {
                ParamName = "ques_srv_survey_id",
                ParamValue = query.SurveyId,
                ParamType = NpgsqlDbType.Uuid,
            };
            INpOnWrapperResult? wrapperResult = await dbFactoryWrapper.QueryAsync(queryString, [param]);

            List<QuestionObject>? questionObjects = wrapperResult?
                .GenericConverter(typeof(QuestionObject))?
                .Cast<QuestionObject>()
                .ToList();

            if (questionObjects is not { Count: > 0 })
            {
                response.SetFail("Không tìm thấy survey");
                return;
            }

            response.Data = questionObjects;
            response.SetSuccess();
        });
    }
}