using CommonObject;
using SSO.OutputModels;
using SSO.ServiceModels.Survey;

namespace SSO.Mappings.Survey;

public static class SurveyModelMapping
{
    public static SurveyModel? ToSurveyModel(this List<QuestionGetBySurveyModel>? flatData)
    {
        if (flatData == null || flatData.Count == 0)
        {
            return null;
        }

        // 1. Lấy thông tin Survey từ dòng đầu tiên
        var firstRow = flatData.First();

        // 2. Group dữ liệu theo Question (vì Answer lặp lại Question)
        var groupedByQuestion = flatData
            .GroupBy(row => row.QuestionId)
            .ToList();

        // 3. Xây dựng các Question Model
        var questions = groupedByQuestion.Select(questionGroup =>
        {
            var questionRow = questionGroup.First(); // Lấy thông tin Question từ dòng đầu tiên của nhóm

            return new QuestionSimpleModel
            {
                QuestionId = questionRow.QuestionId.AsDefaultString(),
                QuestionText = questionRow.QuestionQuestionText,
                QuestionOrder = questionRow.QuestionQuestionOrder.GetValueOrDefault(),
                IsRequired = questionRow.QuestionIsRequired.GetValueOrDefault(),
                CreatedAt = questionRow.QuestionCreatedAt.GetValueOrDefault(),
                UpdatedAt = questionRow.QuestionUpdatedAt.GetValueOrDefault(),

                // Options = new QuestionOptionModel
                // {
                //     QuestionOptionId = Guid.Empty // Placeholder
                // },

                // Xây dựng Answer Model từ các dòng trong nhóm Question
                Answers = questionGroup
                    .Where(a => a.AnswerId.HasValue) // Lọc những dòng có Answer (không phải câu hỏi dạng text)
                    .Select(answerRow => new AnswerModel
                    {
                        AnswerId = answerRow.AnswerId.AsDefaultString(),
                        Description = answerRow.AnswerDescription,
                        OrderSort = answerRow.AnswerOrderSort.GetValueOrDefault(),
                        Score = answerRow.AnswerScore.GetValueOrDefault(),
                        CreatedAt = answerRow.AnswerCreatedAt.GetValueOrDefault(),
                        QuestionId = questionRow.QuestionId.AsDefaultString(),
                    })
                    .ToArray(),
                SurveyId = firstRow.SurveyId.AsDefaultString(),
                Options = null,
            };
        }).ToArray();

        // 4. Xây dựng Survey Model cuối cùng
        return new SurveyModel
        {
            SurveyId = firstRow.SurveyId.AsDefaultString(),
            Title = firstRow.SurveyTitle,
            Description = firstRow.SurveyDescription,
            // IsPublished = firstRow.SurveyIsPublished.AsDefaultBool(),
            CreatedAt = firstRow.SurveyCreatedAt.GetValueOrDefault(),
            ExpiredAt = firstRow.SurveyExpiredAt.GetValueOrDefault(),
            UpdatedAt = firstRow.SurveyUpdatedAt.GetValueOrDefault(),
            Questions = questions
        };
    }
}