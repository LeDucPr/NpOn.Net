using ProtoBuf;

namespace Common.Infrastructures.NpOn.CommonDb.DbResults.Grpc;

[ProtoContract]
public class NpOnGrpcRow : INpOnGrpcObject
{
    [ProtoMember(1)] public Dictionary<string, NpOnGrpcCell> Cells { get; set; } = new();
}

public static class NpOnGrpcRowExtensions
{
    public static NpOnGrpcRow ToGrpcRow(this INpOnRowWrapper? rowWrapper)
    {
        var grpcRow = new NpOnGrpcRow();

        if (rowWrapper == null)
            return grpcRow;

        var cells = rowWrapper.GetRowWrapper();
        foreach (var kvp in cells)
        {
            var grpcCell = kvp.Value.ToGrpcCell();
            grpcRow.Cells[kvp.Key] = grpcCell;
        }

        return grpcRow;
    }
    
    // khi chuyển đổi ngược thì không đáp ứng cqrs ???
    // public static IReadOnlyDictionary<string, NpOnGrpcCell> FromGrpcRow(this NpOnGrpcRow grpcRow)
    // {
    //     var dict = new Dictionary<string, NpOnGrpcCell>();
    //
    //     if (grpcRow?.Cells == null)
    //         return dict;
    //
    //     foreach (var kvp in grpcRow.Cells)
    //     {
    //         // kvp.Key = tên cột, kvp.Value = NpOnGrpcCell
    //         dict[kvp.Key] = kvp.Value;
    //     }
    //
    //     return dict;
    // }
}