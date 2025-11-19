using ProtoBuf;

namespace QuestionServiceObject.QueryObjects;

[ProtoContract]
public class SurveyGetAllQuery : BaseQuestionQuery
{
    [ProtoMember(1)] public required string SurveyIdAsString { get; set; }
    public Guid? SurveyId =>  SurveyIdAsString == "" ? null : Guid.Parse(SurveyIdAsString);
}