using System.Data;
using System.Globalization;
using System.Text;
using ProtoBuf;
using CommonObject;

namespace CommonDb.DbResults.Grpc;

[ProtoContract]
public class NpOnGrpcCell : INpOnGrpcObject
{
    [ProtoMember(1)] public byte[]? ValueBytes { get; set; } // dữ liệu gốc
    [ProtoMember(2)] public DbType DbType { get; set; } // kiểu DbType
    [ProtoMember(3)] public string? ValueTypeName { get; set; } // tên kiểu .NET
    [ProtoMember(4)] public string? SourceTypeName { get; set; }
}

public static class NpOnGrpcCellExtensions
{
    public static NpOnGrpcCell ToGrpcCell(this INpOnCell? cell)
    {
        if (cell?.ValueAsObject == null)
            return new NpOnGrpcCell();

        var type = cell.ValueType;
        var value = cell.ValueAsObject;
        byte[] bytes;

        if (type == typeof(string))
            bytes = Encoding.UTF8.GetBytes((string)value);
        else if (type == typeof(byte[])) // if byte[]
            bytes = (byte[])value;
        // If value is Primitive type BitConverter
        else if (type == typeof(int))
            bytes = BitConverter.GetBytes((int)Convert.ChangeType(value, typeof(int)));
        else if (type == typeof(long))
            bytes = BitConverter.GetBytes((long)Convert.ChangeType(value, typeof(long)));
        else if (type == typeof(short))
            bytes = BitConverter.GetBytes((short)Convert.ChangeType(value, typeof(short)));
        else if (type == typeof(double))
            bytes = BitConverter.GetBytes((double)Convert.ChangeType(value, typeof(double)));
        else if (type == typeof(float))
            bytes = BitConverter.GetBytes((float)Convert.ChangeType(value, typeof(float)));
        else if (type == typeof(bool))
            bytes = BitConverter.GetBytes((bool)Convert.ChangeType(value, typeof(bool)));
        else if (type == typeof(char))
            bytes = BitConverter.GetBytes((char)Convert.ChangeType(value, typeof(char)));
        else if (type == typeof(decimal)) // decimal not has BitConverter → convert to string
            bytes = Encoding.UTF8.GetBytes(((decimal)value).ToString(CultureInfo.InvariantCulture));
        else if (type == typeof(DateTime)) // DateTime → ticks
            bytes = BitConverter.GetBytes(((DateTime)value).Ticks);
        else if (type == typeof(Guid))
            bytes = ((Guid)value).ToByteArray();
        else
            bytes = Encoding.UTF8.GetBytes(JsonConverter.ToJson(value));

        return new NpOnGrpcCell
        {
            ValueBytes = bytes,
            ValueTypeName = type.AssemblyQualifiedName,
            DbType = cell.DbType,
            SourceTypeName = cell.SourceTypeName
        };
    }

    public static object? FromGrpcCell(this NpOnGrpcCell grpcCell)
    {
        if (grpcCell?.ValueBytes == null || string.IsNullOrEmpty(grpcCell.ValueTypeName))
            return null;
        var type = Type.GetType(grpcCell.ValueTypeName);
        if (type == null)
            return null;

        var bytes = grpcCell.ValueBytes;

        if (type == typeof(string))
            return Encoding.UTF8.GetString(bytes);
        if (type == typeof(byte[]))
            return bytes;

        if (type == typeof(int))
            return BitConverter.ToInt32(bytes, 0);
        if (type == typeof(long))
            return BitConverter.ToInt64(bytes, 0);
        if (type == typeof(short))
            return BitConverter.ToInt16(bytes, 0);
        if (type == typeof(double))
            return BitConverter.ToDouble(bytes, 0);
        if (type == typeof(float))
            return BitConverter.ToSingle(bytes, 0);
        if (type == typeof(bool))
            return BitConverter.ToBoolean(bytes, 0);
        if (type == typeof(char))
            return BitConverter.ToChar(bytes, 0);
        if (type == typeof(decimal))
        {
            var str = Encoding.UTF8.GetString(bytes);
            return decimal.Parse(str, CultureInfo.InvariantCulture);
        }

        if (type == typeof(DateTime))
        {
            var ticks = BitConverter.ToInt64(bytes, 0);
            return new DateTime(ticks);
        }

        if (type == typeof(Guid))
            return new Guid(bytes);
        var json = Encoding.UTF8.GetString(bytes); // fallback: JSON deserialize
        return JsonConverter.FromJson(json, type);
    }
}