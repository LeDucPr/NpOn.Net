using System.Reflection;
using HandlerFlow.AlgObjs.CtrlObjs;
using Microsoft.VisualBasic.CompilerServices;

namespace HandlerFlow.AlgObjs.RaisingRouters;

public class AdvancedTypeKey : IEquatable<AdvancedTypeKey>
{
    public string ConcatStringKeyValue { get; }
    public Type CtrlType { get; }

    public AdvancedTypeKey(Dictionary<string, string> fieldMap, Type ctrlType)
    {
        if (!ctrlType.IsChildOfBaseCtrl())
            throw new IncompleteInitialization();

        CtrlType = ctrlType;
        ConcatStringKeyValue = string.Concat(fieldMap.Select(kv => $"{kv.Key}:{kv.Value};"));
    }

    /// <summary>
    /// Compare CtrlType and ConcatStringKeyValue to make key of Object (in cache)
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(AdvancedTypeKey? other)
    {
        if (other is null) return false;
        return CtrlType == other.CtrlType &&
               ConcatStringKeyValue == other.ConcatStringKeyValue;
    }

    public override bool Equals(object? obj) => Equals(obj as AdvancedTypeKey);

    public override int GetHashCode() =>
        HashCode.Combine(CtrlType, ConcatStringKeyValue);
}

public record FieldInfo(Dictionary<string, PropertyInfo> KeyProperties);