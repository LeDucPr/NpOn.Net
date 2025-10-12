namespace CommonDb;

public class NpOnColumnSchemaInfo
{
    public string ColumnName { get; }
    public Type DataType { get; }
    public string ProviderDataTypeName { get; } // Ví dụ: "text", "int4"

    public NpOnColumnSchemaInfo(string columnName, Type dataType, string providerDataTypeName)
    {
        ColumnName = columnName;
        DataType = dataType;
        ProviderDataTypeName = providerDataTypeName;
    }
}