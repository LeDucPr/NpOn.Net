using ProtoBuf;

namespace QuestionServiceObject.QueryObjects;

[ProtoContract]
public class QuestionGetBySurveyIdQuery : BaseQuestionQuery
{
    [ProtoMember(1)] public required string SurveyId { get; set; } // Guid
}

public class QuestionGetByUserIdAndSurveyIdQuery : BaseQuestionQuery
{
    [ProtoMember(1)] public required string SurveyId { get; set; } // Guid
    [ProtoMember(2)] public required string UserId { get; set; } // Guid
}

