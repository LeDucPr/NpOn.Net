namespace Algorithm.AlgObjs.Ctrls;

public record EntityRelationCtrl : BaseCtrl
{
    public required string TargetEntityId { get; set; }
    public required string SourceEntityId { get; set; }
    public string? RelationKind { get; set; }
    public string? JoinType { get; set; }
    public string? JoinCondition { get; set; }
    public string? FilterCondition { get; set; }
    public string? Projection { get; set; } // json
    public string? OrderBy { get; set; }
    public string? Attributes { get; set; } // json
    public string? Notes { get; set; }
}
