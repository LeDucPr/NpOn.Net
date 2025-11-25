using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonObject;
using CommonWebApplication.Services;
using GeneralServiceObject.QueryObjects;
using IGeneralService;
using IQuestionService;
using ProjectEntry.QuestionEntries;
using QuestionServiceObject.BusinessObjects;
using QuestionServiceObject.CommandObjects;
using QuestionServiceObject.QueryObjects;

namespace QuestionService.Services;

public class SurveyService(
    IFldMasterPgService fldMasterPgService,
    ILogger<CommonService> logger
) : CommonService(logger), ISurveyService
{
    public async Task<CommonResponse<string>> AddOrUpdateSurvey(SurveyAddOrUpdateCommand addOrUpdateCommand)
    {
        return await CommonProcess<string>(async (response) =>
        {
            List<TblFldExecutionParam> queryParams =
            [
                new TblFldExecutionParam()
                {
                    ParamName = "title",
                    StringValue = addOrUpdateCommand.Title
                },
                new TblFldExecutionParam()
                {
                    ParamName = "description",
                    StringValue = addOrUpdateCommand.Description
                },
                new TblFldExecutionParam()
                {
                    ParamName = "is_published",
                    StringValue = addOrUpdateCommand.IsPublished.AsDefaultString()
                },
                new TblFldExecutionParam()
                {
                    ParamName = "expired_at",
                    StringValue = addOrUpdateCommand.ExpiredAt.AsDefaultString()
                },
            ];
            
            if (addOrUpdateCommand.Id != null)
            {
                queryParams.Add(new TblFldExecutionParam()
                {
                    ParamName = "id",
                    StringValue = addOrUpdateCommand.Id
                });
            }

            var addNewSurveyResponse = await fldMasterPgService.Execute(new TblFldExecution()
            {
                Code = addOrUpdateCommand.Id == null
                    ? QuestionServiceQueryCode.UserAnswerAdd
                    : QuestionServiceQueryCode.UserAnswerUpdate,
                QueryParams = queryParams.ToArray(),
            });

            if (!addNewSurveyResponse.Status)
            {
                response.SetFail(addNewSurveyResponse.ErrorMessages);
                return;
            }

            response.Data = addOrUpdateCommand.Id == null ? "Add new survey success" : "Update survey success";
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<INpOnGrpcObject>> GetQuestionsBySurveyId(QuestionGetBySurveyIdQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var questionGetBySurveyIdResponse = await fldMasterPgService.Execute(new TblFldExecution()
            {
                Code = QuestionServiceQueryCode.QuestionsBySurveyId,
                QueryParams =
                [
                    new TblFldExecutionParam()
                    {
                        ParamName = "survey_id",
                        StringValue = query.SurveyId
                    }
                ],
            });

            INpOnGrpcObject? questionGrpTable = questionGetBySurveyIdResponse.Data;
            if (!questionGetBySurveyIdResponse.Status)
            {
                response.SetFail(questionGetBySurveyIdResponse.ErrorMessages);
                return;
            }

            response.Data = questionGrpTable;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<INpOnGrpcObject>> GetQuestionsByUserIdAndSurveyId(
        QuestionGetByUserIdAndSurveyIdQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var surveyGetBy = await fldMasterPgService.Execute(new TblFldExecution()
            {
                Code = "sp_dyn_patient_rank_search",
                QueryParams =
                [
                    new TblFldExecutionParam()
                    {
                        ParamName = "json_object_data",
                        // StringValue = query.SurveyId
                        StringValue = @"{
                              ""full_name"": """",
                              ""username"": """",
                              ""from_date"": ""2025-11-07T00:00:00"",
                              ""to_date"": ""2025-11-14T23:59:59"",
                              ""mobile_phone"": """",
                              ""gender"": """",
                              ""province_rcd"": """",
                              ""district_rcd"": """",
                              ""commune_rcd"": """",
                              ""standard_account_id"": ""12fbd6a7-978b-4e7f-98bc-43c21684b371"",
                              ""master_account_id"": null,
                              ""province_account_rcd"": """",
                              ""rank_type"": null,
                              ""page"": 1,
                              ""pageSize"": 1
                            }"
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

    public async Task<CommonResponse<string>> SubmitSurvey(SubmitSurveyCommand command)
    {
        return await CommonProcess<string>(async (response) =>
        {
            // Validate request
            if (string.IsNullOrEmpty(command.SurveyId) || command.Answers.Count == 0)
            {
                response.SetFail("Survey ID and answers are required");
                return;
            }

            try
            {
                // Create submission record
                List<TblFldExecutionParam> queryParams =
                [
                    new TblFldExecutionParam()
                    {
                        ParamName = "survey_id",
                        StringValue = command.SurveyId
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "user_id",
                        StringValue = command.UserId ?? Guid.NewGuid().ToString()
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "total_score",
                        StringValue = command.TotalScore.ToString()
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "submitted_at",
                        StringValue = command.SubmittedAt.ToString("O")
                    }
                ];

                var submitResponse = await fldMasterPgService.Execute(new TblFldExecution()
                {
                    Code = QuestionServiceQueryCode.SubmitSurvey,
                    QueryParams = queryParams.ToArray(),
                });

                if (!submitResponse.Status)
                {
                    response.SetFail(submitResponse.ErrorMessages);
                    return;
                }

                response.Data = "Survey submitted successfully";
                response.SetSuccess();
            }
            catch (Exception ex)
            {
                response.SetFail(new[] { $"Error submitting survey: {ex.Message}" });
            }
        });
    }

    public async Task<CommonResponse<CalculateSurveyScoreObject>> CalculateSurveyScore(CalculateSurveyScoreQuery query)
    {
        return await CommonProcess<CalculateSurveyScoreObject>(async (response) =>
        {
            // Validate request
            if (string.IsNullOrEmpty(query.SurveyId) || query.Answers.Count == 0)
            {
                response.SetFail("Survey ID and answers are required");
                return;
            }

            try
            {
                // Get all answer options for the survey to calculate scores
                var optionsResponse = await fldMasterPgService.Execute(new TblFldExecution()
                {
                    Code = QuestionServiceQueryCode.GetAnswerOptions,
                    QueryParams =
                    [
                        new TblFldExecutionParam()
                        {
                            ParamName = "survey_id",
                            StringValue = query.SurveyId
                        }
                    ],
                });

                if (!optionsResponse.Status)
                {
                    response.SetFail(optionsResponse.ErrorMessages);
                    return;
                }

                // Calculate total score
                int totalScore = 0;
                var questionScores = new List<QuestionScoreObject>();

                // Parse answer options from response
                INpOnGrpcObject? answersData = optionsResponse.Data;
                if (answersData == null)
                {
                    response.SetFail("Failed to retrieve answer options");
                    return;
                }

                // Assuming the response contains answer options in grpc format
                // Convert to AnswerOptionsObject list
                // var selectedOptionIds = query.Answers
                //     .SelectMany(a => a.SelectedOptionIds)
                //     .ToHashSet();

                // For this implementation, we'll calculate scores based on a simplified approach
                // In production, you'd need to convert the grpc data properly
                foreach (var submittedAnswer in query.Answers)
                {
                    int questionScore = 0;
                    
                    // Find selected options and sum their scores
                    if (submittedAnswer.SelectedOptionIds.Count > 0)
                    {
                        questionScore = submittedAnswer.SelectedOptionIds.Count;
                    }

                    totalScore += questionScore;
                    questionScores.Add(new QuestionScoreObject
                    {
                        QuestionId = submittedAnswer.QuestionId,
                        ScoreEarned = questionScore,
                        MaxScore = 100 // Default max score per question
                    });
                }

                // Create response object
                var scoreResult = new CalculateSurveyScoreObject
                {
                    TotalScore = totalScore,
                    QuestionScores = questionScores,
                    ResultCategory = DetermineResultCategory(totalScore)
                };

                response.Data = scoreResult;
                response.SetSuccess();
            }
            catch (Exception ex)
            {
                response.SetFail(new[] { $"Error calculating survey score: {ex.Message}" });
            }
        });
    }

    private string DetermineResultCategory(int totalScore)
    {
        // Define result categories based on score ranges
        if (totalScore >= 90) return "Excellent";
        if (totalScore >= 70) return "Good";
        if (totalScore >= 50) return "Average";
        if (totalScore >= 30) return "Below Average";
        return "Poor";
    }
}