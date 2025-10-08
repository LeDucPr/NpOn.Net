using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.CtrlObjs
{
    public class BaseCtrl
    {
        [Pk(nameof(BaseCtrl.Id))]
        public required string Id { get; set; }
        public DateTime? CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; } 
    }
}