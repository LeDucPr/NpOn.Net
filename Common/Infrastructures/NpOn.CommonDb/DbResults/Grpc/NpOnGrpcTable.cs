using ProtoBuf;

namespace Common.Infrastructures.NpOn.CommonDb.DbResults.Grpc;

[ProtoContract]
public class NpOnGrpcTable : INpOnGrpcObject
{
    [ProtoMember(1)] public Dictionary<int, NpOnGrpcRow>? Rows { get; set; }

    [ProtoMember(2)] public int TotalRow { get; set; }

    // Column Name
    [ProtoMember(3)] public Dictionary<string, NpOnGrpcColumn>? ColumnsByName { get; set; }

    // Column Index
    [ProtoMember(4)] public Dictionary<int, NpOnGrpcColumn>? ColumnsByIndex { get; set; }
}

public static class NpOnGrpcTableExtensions
{
    public static NpOnGrpcTable ToGrpcTable(this INpOnTableWrapper tableWrapper)
    {
        var grpcTable = new NpOnGrpcTable
        { 
            TotalRow = tableWrapper?.RowWrappers?.Count ?? 0,
            Rows = new Dictionary<int, NpOnGrpcRow>(),
            ColumnsByName = new Dictionary<string, NpOnGrpcColumn>(),
            ColumnsByIndex = new Dictionary<int, NpOnGrpcColumn>()
    };

        // Rows
        if (tableWrapper?.RowWrappers != null)
        {
            foreach (var kvp in tableWrapper.RowWrappers)
            {
                if (kvp.Value != null)
                {
                    grpcTable.Rows[kvp.Key] = kvp.Value.ToGrpcRow();
                }
            }
        }

        // Columns (name + index)
        if (tableWrapper?.CollectionWrappers != null)
        {
            var colWrappersByName =
                tableWrapper.CollectionWrappers.GetColumnWrapperByColumnNames(
                    tableWrapper.CollectionWrappers.Keys.ToArray());
            var colWrappersByIndex =
                tableWrapper.CollectionWrappers.GetColumnWrapperByIndexes(tableWrapper.CollectionWrappers.Keys
                    .Select((_, i) => i).ToArray());

            int colIndex = 0;
            if (colWrappersByName != null)
            {
                foreach (var kvp in colWrappersByName)
                {
                    if (kvp.Value != null)
                    {
                        grpcTable.ColumnsByName[kvp.Key] = kvp.Value.ToGrpcColumn(kvp.Key, colIndex);
                    }
                    colIndex++;
                }
            }

            if (colWrappersByIndex != null)
            {
                foreach (var kvp in colWrappersByIndex)
                {
                    if (kvp.Value != null)
                    {
                        grpcTable.ColumnsByIndex[kvp.Key] =
                            kvp.Value.ToGrpcColumn(kvp.Value.GetColumnWrapper().First().Key.ToString(), kvp.Key);
                    }
                }
            }
        }

        return grpcTable;
    }
    public static NpOnGrpcColumn? GetColumnByName(this NpOnGrpcTable? table, string columnName)
    {
        if (table?.ColumnsByName == null || string.IsNullOrEmpty(columnName))
            return null;

        return table.ColumnsByName.GetValueOrDefault(columnName);
    }

    // Lấy cột theo index
    public static NpOnGrpcColumn? GetColumnByIndex(this NpOnGrpcTable? table, int columnIndex)
    {
        return table?.ColumnsByIndex?.GetValueOrDefault(columnIndex);
    }
}