namespace Algorithm.AlgObjs.Ctrls;

public record BaseCtrl
{
    public required string Id { get; set; }
    public DateTime? CreatedAt { get; set; } 
    public DateTime? UpdatedAt { get; set; } 
}