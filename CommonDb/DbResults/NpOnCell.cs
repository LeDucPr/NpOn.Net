using System.Data;

namespace CommonDb.DbResults;

/// <summary>
/// Primitive Data
/// </summary>
public interface INpOnCell
{
    object? ValueAsObject { get; } // Value of Cell
    Type ValueType { get; } // type of System .Net
    DbType DbType { get; } // type of ADO.Net 
    string SourceTypeName { get; }
}

public interface INpOnCell<out T> : INpOnCell
{
    T? Value { get; }
}

/// <summary>
/// Use for Primitive values
/// </summary>
/// <typeparam name="T"></typeparam>
public class NpOnCell<T> : INpOnCell<T>
{
    public T? Value { get; }
    public Type ValueType => typeof(T);
    public DbType DbType { get; }
    public string SourceTypeName { get; }

    public object? ValueAsObject => Value;

    public NpOnCell(T? value, DbType dbType, string sourceTypeName)
    {
        Value = value;
        DbType = dbType;
        SourceTypeName = sourceTypeName;
    }
}