using ObjectHandlerFlow.AlgObjs.Attributes;
using ProtoBuf;

namespace QuestionServiceObject.BusinessObjects;

[ProtoContract]
public class CalculateSurveyScoreObject : BaseQuestionObject
{
    [ProtoMember(1)] public int TotalScore { get; set; }
    [ProtoMember(2)] public List<QuestionScoreObject> QuestionScores { get; set; } = [];
    [ProtoMember(3)] public string? ResultCategory { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new Dictionary<string, string>();
        FieldMap.Add(nameof(TotalScore), "total_score");
        FieldMap.Add(nameof(ResultCategory), "result_category");
        base.FieldMapper();
    }
}

[ProtoContract]
public class QuestionScoreObject
{
    [ProtoMember(1)] public string QuestionId { get; set; }
    [ProtoMember(2)] public int ScoreEarned { get; set; }
    [ProtoMember(3)] public int MaxScore { get; set; }
}
