namespace HandlerFlow.AlgObjs.CtrlObjs;

public record SinkCtrl : BaseCtrl
{
    public string? Name { get; set; }
    public string? SinkType { get; set; }
    public required string EntityId { get; set; }
    public string? TargetPath { get; set; }
    public string? HttpMethod { get; set; }
    public string? Mapping { get; set; } // json
    public string? Attributes { get; set; } // json
    public string? Description { get; set; }
}
