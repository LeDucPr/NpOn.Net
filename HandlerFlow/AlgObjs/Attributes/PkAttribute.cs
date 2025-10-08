using System.ComponentModel.DataAnnotations;
using HandlerFlow.AlgObjs.CtrlObjs;

namespace HandlerFlow.AlgObjs.Attributes;

/// <summary>
/// Single key
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public sealed class PkAttribute : Attribute
{
    public string? PrimaryKeyName { get; }
    public PkAttribute(string primaryKeyName)
    {
        if (string.IsNullOrWhiteSpace(primaryKeyName))
            throw new ArgumentNullException(nameof(primaryKeyName));
        PrimaryKeyName = primaryKeyName;
    }
}

/// <summary>
/// Multiple keys
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public sealed class PkAttribute<T> : Attribute where T : BaseCtrl
{
    public Type RelatedType => typeof(T);
    public string? PrimaryKeyName { get; }

    public PkAttribute(string primaryKeyName)
    {
        if (string.IsNullOrWhiteSpace(primaryKeyName))
            throw new ArgumentNullException(nameof(primaryKeyName));
        PrimaryKeyName = primaryKeyName;
    }
}