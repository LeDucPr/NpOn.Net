namespace CommonDb.DbResults;

/// <summary>
/// Đại diện cho một hàng dữ liệu, cho phép truy cập các ô theo tên cột.
/// </summary>
public interface INpOnRowWrapper
{
    IReadOnlyDictionary<string, INpOnCell?> GetRowWrapper();
}

public interface INpOnColumnWrapper
{
    IReadOnlyDictionary<int, INpOnCell?> GetColumnWrapper();
}

public interface INpOnCollectionWrapper
{
    IReadOnlyDictionary<int, INpOnColumnWrapper?> GetColumnWrapperByIndex(int index);
    IReadOnlyDictionary<string, INpOnColumnWrapper?> GetColumnWrapperByColumnName(string columnName);
}

public interface INpOnTableWrapper
{
    IReadOnlyDictionary<string, INpOnCell?> GetTableWrapper();
    IReadOnlyDictionary<int, INpOnCell?> GetCollectionWrapper();
}