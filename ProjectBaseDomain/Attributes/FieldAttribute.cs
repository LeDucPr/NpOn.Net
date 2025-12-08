namespace ProjectBaseDomain.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FieldAttribute(string fieldName) : Attribute
{
    public string FieldName { get; } = fieldName;
}