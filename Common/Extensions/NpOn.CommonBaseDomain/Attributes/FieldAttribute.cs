namespace Common.Extensions.NpOn.CommonBaseDomain.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FieldAttribute(string fieldName) : Attribute
{
    public string FieldName { get; } = fieldName;
}