namespace HandlerFlow.AlgObjs.CtrlObjs;

public record EntityCtrl : BaseCtrl
{
    public required string DatastoreId { get; set; }
    public required string SchemaId { get; set; }
    public string? Name { get; set; }
    public string? EntityType { get; set; }
    public string? Description { get; set; }
    public string? Attributes { get; set; } // json
}
