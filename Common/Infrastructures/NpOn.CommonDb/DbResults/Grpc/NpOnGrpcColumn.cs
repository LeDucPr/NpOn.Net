using ProtoBuf;

namespace Common.Infrastructures.NpOn.CommonDb.DbResults.Grpc;

[ProtoContract]
public class NpOnGrpcColumn : INpOnGrpcObject
{
    // Cells trong cột này, key = chỉ số hàng
    [ProtoMember(1)] public Dictionary<int, NpOnGrpcCell> Cells { get; set; } = new();

    // Metadata về cột
    [ProtoMember(2)] public string ColumnName { get; set; } = string.Empty;
    [ProtoMember(3)] public int ColumnIndex { get; set; }
}

public static class NpOnGrpcColumnExtensions
{
    public static NpOnGrpcColumn ToGrpcColumn(this INpOnColumnWrapper columnWrapper, string columnName, int columnIndex)
    {
        var grpcColumn = new NpOnGrpcColumn
        {
            ColumnName = columnName,
            ColumnIndex = columnIndex
        };

        foreach (var kvp in columnWrapper.GetColumnWrapper())
        {
            // kvp.Key = chỉ số hàng, kvp.Value = INpOnCell
            grpcColumn.Cells[kvp.Key] = kvp.Value.ToGrpcCell();
        }

        return grpcColumn;
    }
}
