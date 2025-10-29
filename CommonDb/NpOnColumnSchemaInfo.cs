using ProtoBuf;

namespace CommonDb;

[ProtoContract]
public class NpOnColumnSchemaInfo
{
    [ProtoMember(1)] public string ColumnName { get; }
    [ProtoMember(2)] public Type DataType { get; }
    [ProtoMember(3)] public string ProviderDataTypeName { get; } // Ví dụ: "text", "int4"

    public NpOnColumnSchemaInfo(string columnName, Type dataType, string providerDataTypeName)
    {
        ColumnName = columnName;
        DataType = dataType;
        ProviderDataTypeName = providerDataTypeName;
    }
}