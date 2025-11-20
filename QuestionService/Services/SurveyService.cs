using CommonDb.DbCommands;
using CommonDb.DbResults;
using CommonGrpcObject;
using CommonWebApplication.Services;
using DbFactory;
using HandleFlow.ResultConverters;
using IQuestionService;
using NpgsqlTypes;
using QuestionServiceObject.BusinessObjects;
using QuestionServiceObject.QueryObjects;

namespace QuestionService.Services;

public class SurveyService(
    IDbFactoryWrapper dbFactoryWrapper,
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

    /// <summary>
    /// Lấy thông tin chi tiết survey theo ID
    /// </summary>
    public async Task<CommonResponse<QuesSrvDiseaseObject>> GetSurveyById(Guid surveyId)
    {
        return await CommonProcess<QuesSrvDiseaseObject>(async (response) =>
        {
            string pgQuery = @"
                    SELECT 
                        s.id,
                        s.title,
                        s.description,
                        s.max_total_score,
                        s.is_published,
                        s.created_at,
                        s.updated_at,
                        COUNT(DISTINCT q.id) AS question_count,
                        COUNT(DISTINCT ss.id) FILTER (WHERE ss.status = 'submitted') AS total_submissions,
                        COALESCE(AVG(ss.total_score) FILTER (WHERE ss.status = 'submitted'), 0) AS average_score
                    FROM surveys s
                    LEFT JOIN questions q ON s.id = q.survey_id
                    LEFT JOIN survey_submissions ss ON s.id = ss.survey_id
                    WHERE s.id = @survey_id
                    GROUP BY s.id, s.title, s.description, s.max_total_score, s.is_published, s.created_at, s.updated_at";

            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.QueryAsync(pgQuery);
            //new Dictionary<string, object>
            //{
            //    ["survey_id"] = surveyId
            //}

            List<QuesSrvDiseaseObjectDetailObject>? qSdObjects = resultOfQuery?
                .GenericConverter(typeof(QuesSrvDiseaseObjectDetailObject))?
                .Cast<QuesSrvDiseaseObjectDetailObject>()
                .ToList();

            if (qSdObjects is not { Count: > 0 })
            {
                response.SetFail("Không tìm thấy survey");
                return;
            }

            response.Data = qSdObjects.First();
            response.SetSuccess();
        });
    }

    /// <summary>
    /// Lấy survey với đầy đủ questions và options
    /// </summary>
    public async Task<CommonResponse<QuesSrvDiseaseFullObject>> GetSurveyWithQuestions(Guid surveyId)
    {
        return await CommonProcess<QuesSrvDiseaseFullObject>(async (response) =>
        {
            // 1. Lấy thông tin survey
            string surveySql = @"
                    SELECT 
                        id,
                        title,
                        description,
                        max_total_score,
                        is_published,
                        created_at,
                        updated_at
                    FROM surveys
                    WHERE id = @survey_id";

            INpOnWrapperResult? surveyResult = await dbFactoryWrapper.QueryAsync(surveySql);
            //new Dictionary<string, object> { ["survey_id"] = surveyId }

            List<QuesSrvDiseaseObject>? surveyObjects = surveyResult?
                .GenericConverter(typeof(QuesSrvDiseaseObject))?
                .Cast<QuesSrvDiseaseObject>()
                .ToList();

            if (surveyObjects is not { Count: > 0 })
            {
                response.SetFail("Không tìm thấy survey");
                return;
            }

            QuesSrvDiseaseObject survey = surveyObjects.First();

            // 2. Lấy questions
            string questionsSql = @"
                    SELECT 
                        id,
                        survey_id,
                        question_text,
                        question_type,
                        question_order,
                        is_required,
                        max_score,
                        created_at
                    FROM questions
                    WHERE survey_id = @survey_id
                    ORDER BY question_order";

            INpOnWrapperResult? questionsResult = await dbFactoryWrapper.QueryAsync(questionsSql);
            //new Dictionary<string, object> { ["survey_id"] = surveyId }

            List<QuestionObject>? questionObjects = questionsResult?
                .GenericConverter(typeof(QuestionObject))?
                .Cast<QuestionObject>()
                .ToList();

            // 3. Lấy options cho từng question
            List<QuestionWithOptionsObject> questionsWithOptions = new();

            if (questionObjects is { Count: > 0 })
            {
                foreach (var question in questionObjects)
                {
                    string optionsSql = @"
                            SELECT 
                                id,
                                question_id,
                                option_text,
                                option_order,
                                score_value,
                                created_at
                            FROM answer_options
                            WHERE question_id = @question_id
                            ORDER BY option_order";

                    INpOnWrapperResult? optionsResult = await dbFactoryWrapper.QueryAsync(optionsSql);
                    //new Dictionary<string, object> { ["question_id"] = question.Id }

                    List<AnswerOptionsObject>? optionObjects = optionsResult?
                        .GenericConverter(typeof(AnswerOptionsObject))?
                        .Cast<AnswerOptionsObject>()
                        .ToList();

                    questionsWithOptions.Add(new QuestionWithOptionsObject
                    {
                        //Id = question.Id,
                        SurveyId = question.SurveyId,
                        QuestionText = question.QuestionText,
                        QuestionType = question.QuestionType,
                        QuestionOrder = question.QuestionOrder,
                        IsRequired = question.IsRequired,
                        MaxScore = question.MaxScore,
                        //CreatedAt = question.CreatedAt,
                        Options = optionObjects ?? new List<AnswerOptionsObject>()
                    });
                }
            }

            // 4. Lấy result categories
            string categoriesSql = @"
                    SELECT 
                        id,
                        survey_id,
                        category_name,
                        description,
                        min_score,
                        max_score,
                        recommendation,
                        severity_level,
                        color_hex,
                        display_order,
                        created_at
                    FROM result_categories
                    WHERE survey_id = @survey_id
                    ORDER BY display_order";

            INpOnWrapperResult? categoriesResult = await dbFactoryWrapper.QueryAsync(categoriesSql);
            //new Dictionary<string, object> { ["survey_id"] = surveyId }

            List<ResultCategoriesObject>? categoryObjects = categoriesResult?
                .GenericConverter(typeof(ResultCategoriesObject))?
                .Cast<ResultCategoriesObject>()
                .ToList();

            // 5. Tạo SurveyFullObject
            var fullSurvey = new QuesSrvDiseaseFullObject
            {
                //Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                CreatedAt = survey.CreatedAt,
                UpdatedAt = survey.UpdatedAt,
                Questions = questionsWithOptions,
                ResultCategories = categoryObjects ?? new List<ResultCategoriesObject>()
            };

            response.Data = fullSurvey;
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

    public Task<CommonResponse<QuestionWithOptionsObject>> GetQuestionWithOptions(Guid questionId)
    {
        throw new NotImplementedException();
    }
}