using CommonDb.DbResults;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs.Data;
using System.Data;

namespace SystemController.ResultConverters;

public class JoiningTable
{
    public List<INpOnWrapperResult>? QueryResults;
    public List<UnifiedTableMappingCtrl>? TableMappings;

    public JoiningTable(List<INpOnWrapperResult> queryResults,
        List<UnifiedTableMappingCtrl> tableMappings)
    {
        QueryResults = queryResults;
        TableMappings = tableMappings;
    }

    /// <summary>
    /// Thực hiện join dữ liệu trong bộ nhớ giữa hai bảng (đại diện bởi INpOnTableWrapper)
    /// dựa trên một quy tắc mapping cụ thể.
    /// </summary>
    /// <param name="leftTable">Bảng bên trái của phép join.</param>
    /// <param name="rightTable">Bảng bên phải của phép join.</param>
    /// <param name="mappingRule">Quy tắc mapping chứa thông tin về join.</param>
    /// <returns>Một bảng kết quả mới sau khi đã join.</returns>
    /// <exception cref="InvalidOperationException">Ném ra khi dữ liệu đầu vào không hợp lệ.</exception>
    private INpOnTableWrapper? ExecuteInMemoryJoin(
        INpOnTableWrapper leftTable,
        INpOnTableWrapper rightTable,
        UnifiedTableMappingCtrl mappingRule)
    {
        // 1. Xác thực các thông tin cần thiết cho việc join
        if (mappingRule.JoinType == null)
            throw new InvalidOperationException("JoinType is required for an in-memory join.");

        // Lấy thông tin cột join từ bảng bên trái (bảng nguồn của điều kiện)
        var leftJoinColumnName = mappingRule.JoinTableField?.FieldName;
        if (string.IsNullOrEmpty(leftJoinColumnName))
            throw new InvalidOperationException("The source join column (JoinTableField.FieldName) is not specified.");

        // Lấy thông tin cột join từ bảng bên phải (bảng được join vào)
        var rightJoinColumnName = mappingRule.TableField?.FieldName;
        if (string.IsNullOrEmpty(rightJoinColumnName))
            throw new InvalidOperationException("The target join column (TableField.FieldName) is not specified.");

        // 2. Tạo một Lookup từ bảng bên phải để tra cứu hiệu quả (Hash Join)
        // Key là giá trị của cột join, Value là toàn bộ hàng dữ liệu.
        var rightTableLookup = rightTable.RowWrappers
            .ToLookup(
                keySelector: row => row.Value?.GetRowWrapper()[rightJoinColumnName]?.ValueAsObject,
                elementSelector: row => row.Value
            );

        var newJoinedRows = new List<INpOnRowWrapper>();

        // 3. Lặp qua từng hàng của bảng bên trái để thực hiện join
        foreach (var leftRowWrapper in leftTable.RowWrappers.Values)
        {
            var leftRowData = leftRowWrapper.GetRowWrapper();
            object? leftJoinValue = leftRowData[leftJoinColumnName]?.ValueAsObject;

            // Tìm các hàng khớp trong bảng bên phải
            var matchingRightRows = rightTableLookup[leftJoinValue].ToList();

            if (matchingRightRows.Any())
            {
                // Với mỗi hàng khớp, tạo một hàng mới đã được join
                foreach (var rightRow in matchingRightRows)
                {
                    var newRow = new Dictionary<string, INpOnCell>(leftRowData);
                    // Thêm các cột từ hàng bên phải vào, tránh ghi đè nếu có cột trùng tên
                    foreach (var rightCell in rightRow.GetRowWrapper())
                    {
                        newRow.TryAdd(rightCell.Key, rightCell.Value);
                    }
                    // TODO: Cần một lớp triển khai INpOnRowWrapper để chứa newRow
                    // newJoinedRows.Add(new NpOnRowWrapper(newRow));
                }
            }
            else if (mappingRule.JoinType == EDbJoinType.LeftJoin || mappingRule.JoinType == EDbJoinType.LeftOuterJoin)
            {
                // Xử lý cho Left Join: nếu không có hàng nào khớp, vẫn giữ lại hàng bên trái
                // và thêm các giá trị NULL cho các cột của bảng bên phải.
                var newRow = new Dictionary<string, INpOnCell>(leftRowData);
                foreach (var rightColumnName in rightTable.CollectionWrappers.Keys)
                {
                    // TODO: Cần một lớp triển khai INpOnCell để tạo cell có giá trị NULL
                    // newRow.TryAdd(rightColumnName, new NpOnCell(null, DbType.Object));
                }
                // TODO: Cần một lớp triển khai INpOnRowWrapper để chứa newRow
                // newJoinedRows.Add(new NpOnRowWrapper(newRow));
            }
        }

        // TODO: Cần một lớp triển khai INpOnTableWrapper để trả về kết quả cuối cùng
        // var finalColumns = leftTable.CollectionWrappers.Concat(rightTable.CollectionWrappers).ToDictionary();
        // return new NpOnTableWrapper(newJoinedRows, finalColumns);
        return null; // Trả về null tạm thời
    }

    /// <summary>
    /// Tìm và trả về INpOnTableWrapper từ danh sách kết quả dựa trên tên bảng.
    /// </summary>
    private INpOnTableWrapper? FindTableResultByTableName(string tableName)
    {
        // mỗi INpOnWrapperResult có một cách để xác định tên bảng của nó.
        // Ở đây, ta có thể tạm giả định dựa trên một quy ước nào đó hoặc cần thêm thông tin vào INpOnWrapperResult.
        // Ví dụ: kiểm tra xem có cột nào thuộc về bảng đó không.
        // Đây là một ví dụ đơn giản, bạn có thể cần một logic phức tạp hơn.
        return QueryResults?
            .OfType<INpOnTableWrapper>()
            .FirstOrDefault(t => t.CollectionWrappers.Keys.Any(k => k.StartsWith(tableName, StringComparison.OrdinalIgnoreCase)));
    }
}