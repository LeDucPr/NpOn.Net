using System.Data;
using ProtoBuf;

namespace CommonDb.DbResults;

/// <summary>
/// Primitive Data
/// </summary>
[ProtoContract]
public interface INpOnCell
{
    [ProtoMember(1)] object? ValueAsObject { get; } // Value of Cell
    [ProtoMember(2)] Type ValueType { get; } // type of System .Net
    [ProtoMember(3)] DbType DbType { get; } // type of ADO.Net 
    [ProtoMember(4)] string SourceTypeName { get; }
}

[ProtoContract]
public interface INpOnCell<out T> : INpOnCell
{
    [ProtoMember(1)] T? Value { get; }
}

/// <summary>
/// Use for Primitive values
/// </summary>
/// <typeparam name="T"></typeparam>
[ProtoContract]
public class NpOnCell<T> : INpOnCell<T>
{
    [ProtoMember(1)] public T? Value { get; }
    public Type ValueType => typeof(T);
    [ProtoMember(2)] public DbType DbType { get; }
    [ProtoMember(3)] public string SourceTypeName { get; }
    [ProtoMember(4)] public object? ValueAsObject => Value;

    public NpOnCell(T? value, DbType dbType, string sourceTypeName)
    {
        Value = value;
        DbType = dbType;
        SourceTypeName = sourceTypeName;
    }
}