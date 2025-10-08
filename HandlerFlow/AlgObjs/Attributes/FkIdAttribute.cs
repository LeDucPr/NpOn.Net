using System.ComponentModel.DataAnnotations.Schema;
using HandlerFlow.AlgObjs.CtrlObjs;

namespace HandlerFlow.AlgObjs.Attributes;

public sealed class FkIdAttribute<T> : ForeignKeyAttribute where T : BaseCtrl
{
    public Type RelatedType => typeof(T);
    public string? ForeignKeyName { get; }

    public FkIdAttribute(string foreignKeyName) : base(foreignKeyName)
    {
        if (string.IsNullOrWhiteSpace(foreignKeyName))
            throw new ArgumentNullException(nameof(foreignKeyName));
        ForeignKeyName = foreignKeyName;
    }
}