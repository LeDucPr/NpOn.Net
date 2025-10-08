using System.ComponentModel.DataAnnotations.Schema;
using HandlerFlow.AlgObjs.CtrlObjs;

namespace HandlerFlow.AlgObjs.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Constructor,
    AllowMultiple = true, Inherited = true)]
public sealed class TableLoaderAttribute : Attribute
{
    public string? TableName { get; }

    public TableLoaderAttribute(string tableName)
    {
        TableName = tableName;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Constructor,
    AllowMultiple = true, Inherited = true)]
public sealed class TableLoaderAttribute<T> : TableAttribute where T : BaseCtrl
{
    public Type RelatedType => typeof(T);
    public string? TableName { get; }

    public TableLoaderAttribute(string tableName) : base(tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentNullException(nameof(tableName));
        TableName = tableName;
    }
}