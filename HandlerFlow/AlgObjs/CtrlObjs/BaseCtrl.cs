using System.ComponentModel.DataAnnotations;

namespace HandlerFlow.AlgObjs.CtrlObjs
{
    public record BaseCtrl
    {
        [Key]
        public required string Id { get; set; }
        public DateTime? CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; } 
    }
}