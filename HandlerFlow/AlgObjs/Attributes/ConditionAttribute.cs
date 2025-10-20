using System.Reflection;
using CommonMode;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;

namespace HandlerFlow.AlgObjs.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public sealed class ConditionAttribute<T1, T2> : Attribute where T1 : BaseCtrl where T2 : BaseCtrl
{
    public string ConditionKey1 { get; }
    public string ConditionKey2 { get; }

    /// <summary>
    /// Connect 2 Properties of Object (inherit from BaseCtrl)
    /// </summary>
    /// <param name="conditionKey1"></param>
    /// <param name="conditionKey2"></param>
    /// <exception cref="ArgumentNullException">conditionKey1 || conditionKey2 null/space</exception>
    public ConditionAttribute(string conditionKey1, string conditionKey2)
    {
        if (string.IsNullOrWhiteSpace(conditionKey1))
            throw new ArgumentNullException(nameof(conditionKey1));
        if (string.IsNullOrWhiteSpace(conditionKey2))
            throw new ArgumentNullException(nameof(conditionKey2));
        ConditionKey1 = conditionKey1;
        ConditionKey2 = conditionKey2;
        Validate();
    }

    /// <summary>
    /// ConditionKey1 is PropertyInfo.Name of T1 -and- ConditionKey1 too (for T2).
    /// </summary>
    /// <exception cref="InvalidOperationException">Ném ra nếu một trong các thuộc tính không tồn tại.</exception>
    private void Validate()
    {
        var localProperty = typeof(T1).GetProperty(ConditionKey1,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (localProperty == null)
            throw new InvalidOperationException(
                $"'{ConditionKey1}' {EDbError.FieldNotFound.GetDisplayName()} '{typeof(T1).Name}'.");

        var foreignProperty = typeof(T2).GetProperty(ConditionKey2,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (foreignProperty == null)
            throw new InvalidOperationException(
                $"Thuộc tính '{ConditionKey2}' {EDbError.FieldNotFound.GetDisplayName()} '{typeof(T2).Name}'.");
    }
}