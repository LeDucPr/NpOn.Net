using System.ComponentModel.DataAnnotations.Schema;
using ObjectHandlerFlow.AlgObjs.CtrlObjs;

namespace ObjectHandlerFlow.AlgObjs.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public sealed class FkAttribute<T> : ForeignKeyAttribute where T : BaseCtrl
{
    public Type RelatedType => typeof(T);
    public string? ForeignKeyName { get; }

    public FkAttribute(string foreignKeyName) : base(foreignKeyName)
    {
        if (string.IsNullOrWhiteSpace(foreignKeyName))
            throw new ArgumentNullException(nameof(foreignKeyName));
        ForeignKeyName = foreignKeyName;
    }
}