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
    IReadOnlyDictionary<int, INpOnColumnWrapper?> GetColumnWrapperByIndex(int[] indexes);
    IReadOnlyDictionary<string, INpOnColumnWrapper?> GetColumnWrapperByColumnName(string[] columnNames);
}

public interface INpOnTableWrapper
{
    IReadOnlyDictionary<int, INpOnRowWrapper?> RowWrappers { get; }
    INpOnCollectionWrapper CollectionWrappers { get; }
}