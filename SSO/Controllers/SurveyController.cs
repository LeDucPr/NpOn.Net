using AccountServiceObject.QueryObjects;
using CommonGrpcObject;
using CommonWebApplication.Services;
using Enums;
using IQuestionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionServiceObject.BusinessObjects;
using QuestionServiceObject.QueryObjects;
using SSO.OutputModels;
using SSO.Requests;

namespace SSO.Controllers;

public class SurveyController(
    ILogger<AccountController> logger,
    ContextService contextService,
    ISurveyService surveyService
) : BaseSsoController(logger, contextService)
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<CommonApiResponse<object>> QuestionGetBySurvey([FromBody] QuestionGetBySurveyIdRequest? request)
    {   
        return await ProcessRequest<object>(async (response) =>
        {
            if (request == null)
            {
                response.SetFail(EErrorCode.NullRequestExceptions);
                return;
            }

            var questionsBySurvey = await surveyService.GetQuestionsBySurvey(new SurveyGetAllQuery
            {
                SurveyIdAsString = request.SurveyId,
            });
            List<QuestionObject>? questions = questionsBySurvey.Data;
            // if (!questionsBySurvey.Status)
            // {
            //     response.SetFail(questionsBySurvey.ErrorMessages);
            //     return;
            // }

            List<OutputModels.SurveyModel>? questionModels = questions?.Select(x => new OutputModels.SurveyModel()
            {
                SurveyId = "20000001-1001-1001-1001-100000000001",
                Title = "Đánh giá nguy cơ phụ thuộc thuốc cắt cơn",
                Description = "Bộ câu hỏi đánh giá nguy cơ lệ thuộc bình xịt cắt cơn dựa trên 6 câu trả lời.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddYears(10),
                Questions = new[]
                {
                    // CÂU 1
                    new QuestionSimpleModel
                    {
                        QuestionId = "30000001-0001-0001-0001-100000000001",
                        SurveyId = "20000001-1001-1001-1001-100000000001",
                        QuestionText = "Dùng bình xịt cắt cơn để giảm triệu chứng là cách tốt nhất để kiểm soát Hen.",
                        QuestionOrder = 1,
                        IsRequired = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Options = new QuestionOptionModel
                        {
                            QuestionOptionId = "SingleChoice",
                            Code = "SingleChoice",
                            Description = "Chỉ cho phép chọn 1 câu trả lời",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        Answers = new[]
                        {
                            new AnswerModel { AnswerId = "a1-q1", QuestionId = "30000001-0001-0001-0001-100000000001", Description = "Hoàn toàn không đồng ý", OrderSort = 1, Score = 1, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a2-q1", QuestionId = "30000001-0001-0001-0001-100000000001", Description = "Không đồng ý", OrderSort = 2, Score = 2, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a3-q1", QuestionId = "30000001-0001-0001-0001-100000000001", Description = "Không chắc", OrderSort = 3, Score = 3, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a4-q1", QuestionId = "30000001-0001-0001-0001-100000000001", Description = "Đồng ý", OrderSort = 4, Score = 4, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a5-q1", QuestionId = "30000001-0001-0001-0001-100000000001", Description = "Hoàn toàn đồng ý", OrderSort = 5, Score = 5, CreatedAt = DateTime.UtcNow }
                        }
                    },

                    // CÂU 2
                    new QuestionSimpleModel
                    {
                        QuestionId = "30000001-0001-0001-0002-100000000001",
                        SurveyId = "20000001-1001-1001-1001-100000000001",
                        QuestionText = "Bệnh nhân không lo ngại gì về bệnh Hen khi có bình xịt cắt cơn bên cạnh.",
                        QuestionOrder = 2,
                        IsRequired = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Options = new QuestionOptionModel
                        {
                            QuestionOptionId = "SingleChoice",
                            Code = "SingleChoice",
                            Description = "Chỉ cho phép chọn 1 câu trả lời",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        Answers = new[]
                        {
                            new AnswerModel { AnswerId = "a1-q2", QuestionId = "30000001-0001-0001-0002-100000000001", Description = "Hoàn toàn không đồng ý", OrderSort = 1, Score = 1, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a2-q2", QuestionId = "30000001-0001-0001-0002-100000000001", Description = "Không đồng ý", OrderSort = 2, Score = 2, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a3-q2", QuestionId = "30000001-0001-0001-0002-100000000001", Description = "Không chắc", OrderSort = 3, Score = 3, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a4-q2", QuestionId = "30000001-0001-0001-0002-100000000001", Description = "Đồng ý", OrderSort = 4, Score = 4, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a5-q2", QuestionId = "30000001-0001-0001-0002-100000000001", Description = "Hoàn toàn đồng ý", OrderSort = 5, Score = 5, CreatedAt = DateTime.UtcNow }
                        }
                    },

                    //CÂU 3
                    new QuestionSimpleModel
                    {
                        QuestionId = "30000001-0001-0001-0003-100000000001",
                        SurveyId = "20000001-1001-1001-1001-100000000001",
                        QuestionText = "Bình xịt cắt cơn là điều trị duy nhất mà tôi thật sự tin tưởng.",
                        QuestionOrder = 3,
                        IsRequired = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Options = new QuestionOptionModel
                        {
                            QuestionOptionId = "SingleChoice",
                            Code = "SingleChoice",
                            Description = "Chỉ cho phép chọn 1 câu trả lời",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        Answers = new[]
                        {
                            new AnswerModel { AnswerId = "a1-q3", QuestionId = "30000001-0001-0001-0003-100000000001", Description = "Hoàn toàn không đồng ý", OrderSort = 1, Score = 1, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a2-q3", QuestionId = "30000001-0001-0001-0003-100000000001", Description = "Không đồng ý", OrderSort = 2, Score = 2, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a3-q3", QuestionId = "30000001-0001-0001-0003-100000000001", Description = "Không chắc", OrderSort = 3, Score = 3, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a4-q3", QuestionId = "30000001-0001-0001-0003-100000000001", Description = "Đồng ý", OrderSort = 4, Score = 4, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a5-q3", QuestionId = "30000001-0001-0001-0003-100000000001", Description = "Hoàn toàn đồng ý", OrderSort = 5, Score = 5, CreatedAt = DateTime.UtcNow }
                        }
                    },

                    //CÂU 4
                    new QuestionSimpleModel
                    {
                        QuestionId = "30000001-0001-0001-0004-100000000001",
                        SurveyId = "20000001-1001-1001-1001-100000000001",
                        QuestionText = "Lợi ích của bình xịt cắt cơn thật sự nhiều hơn so với nguy cơ.",
                        QuestionOrder = 4,
                        IsRequired = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Options = new QuestionOptionModel
                        {
                            QuestionOptionId = "SingleChoice",
                            Code = "SingleChoice",
                            Description = "Chỉ cho phép chọn 1 câu trả lời",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        Answers = new[]
                        {
                            new AnswerModel { AnswerId = "a1-q4", QuestionId = "30000001-0001-0001-0004-100000000001", Description = "Hoàn toàn không đồng ý", OrderSort = 1, Score = 1, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a2-q4", QuestionId = "30000001-0001-0001-0004-100000000001", Description = "Không đồng ý", OrderSort = 2, Score = 2, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a3-q4", QuestionId = "30000001-0001-0001-0004-100000000001", Description = "Không chắc", OrderSort = 3, Score = 3, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a4-q4", QuestionId = "30000001-0001-0001-0004-100000000001", Description = "Đồng ý", OrderSort = 4, Score = 4, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a5-q4", QuestionId = "30000001-0001-0001-0004-100000000001", Description = "Hoàn toàn đồng ý", OrderSort = 5, Score = 5, CreatedAt = DateTime.UtcNow }
                        }
                    },

                    //CÂU 5
                    new QuestionSimpleModel
                    {
                        QuestionId = "30000001-0001-0001-0005-100000000001",
                        SurveyId = "20000001-1001-1001-1001-100000000001",
                        QuestionText = "Bệnh nhân ưu tiên lựa chọn bình xịt cắt cơn màu xanh hơn dùng ống hít duy trì chứa corticoid.",
                        QuestionOrder = 5,
                        IsRequired = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Options = new QuestionOptionModel
                        {
                            QuestionOptionId = "SingleChoice",
                            Code = "SingleChoice",
                            Description = "Chỉ cho phép chọn 1 câu trả lời",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        Answers = new[]
                        {
                            new AnswerModel { AnswerId = "a1-q5", QuestionId = "30000001-0001-0001-0005-100000000001", Description = "Hoàn toàn không đồng ý", OrderSort = 1, Score = 1, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a2-q5", QuestionId = "30000001-0001-0001-0005-100000000001", Description = "Không đồng ý", OrderSort = 2, Score = 2, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a3-q5", QuestionId = "30000001-0001-0001-0005-100000000001", Description = "Không chắc", OrderSort = 3, Score = 3, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a4-q5", QuestionId = "30000001-0001-0001-0005-100000000001", Description = "Đồng ý", OrderSort = 4, Score = 4, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a5-q5", QuestionId = "30000001-0001-0001-0005-100000000001", Description = "Hoàn toàn đồng ý", OrderSort = 5, Score = 5, CreatedAt = DateTime.UtcNow }
                        }
                    },

                    //CÂU 6
                    new QuestionSimpleModel
                    {
                        QuestionId = "30000001-0001-0001-0006-100000000001",
                        SurveyId = "20000001-1001-1001-1001-100000000001",
                        QuestionText = "Trong 4 tuần vừa qua, bệnh nhân sử dụng bình xịt cắt cơn như thế nào?",
                        QuestionOrder = 6,
                        IsRequired = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Options = new QuestionOptionModel
                        {
                            QuestionOptionId = "SingleChoice",
                            Code = "SingleChoice",
                            Description = "Chỉ cho phép chọn 1 câu trả lời",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        Answers = new[]
                        {
                            new AnswerModel { AnswerId = "a1-q6", QuestionId = "30000001-0001-0001-0006-100000000001", Description = "Không sử dụng", OrderSort = 1, Score = 1, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a2-q6", QuestionId = "30000001-0001-0001-0006-100000000001", Description = "2 lần/tuần", OrderSort = 2, Score = 2, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a3-q6", QuestionId = "30000001-0001-0001-0006-100000000001", Description = "3 lần/tuần", OrderSort = 3, Score = 3, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a4-q6", QuestionId = "30000001-0001-0001-0006-100000000001", Description = "4-5 lần/tuần", OrderSort = 4, Score = 4, CreatedAt = DateTime.UtcNow },
                            new AnswerModel { AnswerId = "a5-q6", QuestionId = "30000001-0001-0001-0006-100000000001", Description = "> 5 lần/tuần", OrderSort = 5, Score = 5, CreatedAt = DateTime.UtcNow }
                        }
                    }
                }
            }).ToList();

            response.Data = new
            {
                Models = questionModels,
            };
            response.SetSuccess();
        });
    }
}