namespace Algorithm.AlgObjs.Ctrls;

public record DatastoreCtrl : BaseCtrl
{
    public required string ConnectionId { get; set; }
    public string? Name { get; set; }
    public string? StoreType { get; set; }
    public string? Description { get; set; }
    public string? Attributes { get; set; } // json
}