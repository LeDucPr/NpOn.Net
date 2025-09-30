namespace Algorithm.AlgObjs.CtrlObjs;

public record FieldCtrl : BaseCtrl
{
    public required string EntityId { get; set; }
    public string? Name { get; set; }
    public string? DataType { get; set; }
    public int? Length { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool? IsNullable { get; set; }
    public bool? IsPrimaryKey { get; set; }
    public bool? IsUnique { get; set; }
    public string? DefaultExpr { get; set; }
    public string? Description { get; set; }
    public string? Attributes { get; set; } // json
    public int? OrdinalPosition { get; set; }
}
