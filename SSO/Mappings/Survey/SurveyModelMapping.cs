using CommonObject;
using SSO.OutputModels;
using SSO.ServiceModels.Survey;

namespace SSO.Mappings.Survey;

public static class SurveyModelMapping
{
    public static SurveyModel? ToSurveyModel(this List<QuestionGetBySurveyModel>? flatData)
    {
        if (flatData == null || !flatData.Any())
        {
            return null;
        }

        var firstRow = flatData.First();

        var questions = flatData
            .GroupBy(row => row.QuestionId)
            .Select(questionGroup =>
            {
                var questionRow = questionGroup.First();

                return new QuestionSimpleModel
                {
                    QuestionId = questionRow.QuestionId.AsDefaultString(),
                    SurveyId = firstRow.SurveyId.AsDefaultString(),
                    QuestionText = questionRow.QuestionQuestionText,
                    QuestionOrder = questionRow.QuestionQuestionOrder.GetValueOrDefault(),
                    IsRequired = questionRow.QuestionIsRequired.GetValueOrDefault(),
                    CreatedAt = questionRow.QuestionCreatedAt.GetValueOrDefault(),
                    UpdatedAt = questionRow.QuestionUpdatedAt.GetValueOrDefault(),

                    Options = new QuestionOptionModel
                    {
                        QuestionOptionId = questionRow.QuestionOptionId.AsDefaultString(),
                        Code = questionRow.QuestionOptionCode,
                        Description = questionRow.QuestionOptionDescription,
                        CreatedAt = default, 
                        UpdatedAt = default
                    },

                    Answers = questionGroup
                        .Where(a => a.AnswerId.HasValue)
                        .Select(answerRow => new AnswerModel
                        {
                            AnswerId = answerRow.AnswerId.AsDefaultString(),
                            QuestionId = questionRow.QuestionId.AsDefaultString(),
                            Description = answerRow.AnswerDescription,
                            OrderSort = answerRow.AnswerOrderSort.GetValueOrDefault(),
                            Score = answerRow.AnswerScore.GetValueOrDefault(),
                            CreatedAt = answerRow.AnswerCreatedAt.GetValueOrDefault(),
                        })
                        .DistinctBy(a => a.AnswerId)
                        .ToArray(),
                };
            })
            .OrderBy(q => q.QuestionOrder)
            .ToArray();

        return new SurveyModel
        {
            SurveyId = firstRow.SurveyId.AsDefaultString(),
            Title = firstRow.SurveyTitle,
            Description = firstRow.SurveyDescription,
            CreatedAt = firstRow.SurveyCreatedAt.GetValueOrDefault(),
            ExpiredAt = firstRow.SurveyExpiredAt.GetValueOrDefault(),
            UpdatedAt = firstRow.SurveyUpdatedAt.GetValueOrDefault(),
            Questions = questions
        };
    }
}