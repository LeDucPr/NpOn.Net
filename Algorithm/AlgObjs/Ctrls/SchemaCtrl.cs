namespace Algorithm.AlgObjs.Ctrls;

public record SchemaCtrl : BaseCtrl
{
    public required string DatastoreId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Attributes { get; set; } // json
}