namespace CommonDb.DbResults;

public interface INpOnRowWrapper
{
    IReadOnlyDictionary<string, INpOnCell> GetRowWrapper();
}

public interface INpOnColumnWrapper
{
    IReadOnlyDictionary<int, INpOnCell> GetColumnWrapper();
}

public interface INpOnCollectionWrapper
{
    IReadOnlyDictionary<int, INpOnColumnWrapper?> GetColumnWrapperByIndexes(int[] indexes);
    IReadOnlyDictionary<string, INpOnColumnWrapper?> GetColumnWrapperByColumnNames(string[]? columnNames = null);
    IEnumerable<string> Keys { get; }
}

public interface INpOnTableWrapper
{
    IReadOnlyDictionary<int, INpOnRowWrapper?> RowWrappers { get; }
    INpOnCollectionWrapper CollectionWrappers { get; }
}

public interface INpOnSuperTableWrapper : INpOnTableWrapper, INpOnWrapperResult
{
}