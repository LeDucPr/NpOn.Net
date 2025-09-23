namespace CommonDb.DbCommands;

public interface INpOnRow
{
}

public interface INpOnRow<out T> : INpOnRow
{
    int? Index { get; }
    Dictionary<string, object?>? Columns { get; }
    T? DataRow { get; }
}

public abstract class NpOnRow<T> : INpOnRow<T> where T : class
{
    // private readonly Type _rowType = typeof(T);
    private readonly T? _dataRow;
    private readonly int? _index;
    private readonly Dictionary<string, object?>? _columns;

    protected NpOnRow(T dataRow)
    {
        _dataRow = dataRow;
    }

    protected NpOnRow(T dataRow, int index, Dictionary<string, object?> columns)
    {
        _dataRow = dataRow;
        _index = index;
        _columns = columns;
    }

    public int? Index => _index;
    public Dictionary<string, object?>? Columns => _columns;
    public T? DataRow => _dataRow;
}