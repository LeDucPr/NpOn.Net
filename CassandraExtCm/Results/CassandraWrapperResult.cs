using Cassandra;
using CommonDb;
using CommonDb.DbResults;
using Enums;
using PostgresExtCm.Results;

namespace CassandraExtCm.Results;

/// <summary>
/// Cassandra Row -> Cell 
/// </summary>
public class CassandraRowWrapper : NpOnWrapperResult<Row, IReadOnlyDictionary<string, INpOnCell>>, INpOnRowWrapper
{
    private readonly IReadOnlyDictionary<string, NpOnColumnSchemaInfo> _schemaMap;

    public CassandraRowWrapper(Row parent, IReadOnlyDictionary<string, NpOnColumnSchemaInfo> schemaMap) : base(parent)
    {
        _schemaMap = schemaMap;
    }

    protected override IReadOnlyDictionary<string, INpOnCell> CreateResult()
    {
        var dictionary = new Dictionary<string, INpOnCell>();

        foreach (var schemaInfo in _schemaMap.Values)
        {
            // Lấy giá trị từ đối tượng Row của Cassandra bằng tên cột
            object? cellValue = Parent.GetValue(schemaInfo.DataType, schemaInfo.ColumnName);
            Type columnType = schemaInfo.DataType;

            Type genericCellType = typeof(NpOnCell<>).MakeGenericType(columnType);

            INpOnCell cell = (INpOnCell)Activator.CreateInstance(
                genericCellType,
                cellValue,
                columnType.ToDbType(), // Chuyển đổi từ System.Type sang DbType
                schemaInfo.ProviderDataTypeName // Tên kiểu dữ liệu gốc của Cassandra (ví dụ: "text", "int")
            )!;

            dictionary.Add(schemaInfo.ColumnName, cell);
        }

        return dictionary;
    }

    public IReadOnlyDictionary<string, INpOnCell> GetRowWrapper() => Result;
}

/// <summary>
/// Cassandra Column -> Cell 
/// </summary>
public class CassandraColumnWrapper : NpOnWrapperResult<IReadOnlyList<Row>, IReadOnlyDictionary<int, INpOnCell>>,
    INpOnColumnWrapper
{
    private readonly string _columnName;
    private readonly IReadOnlyDictionary<string, NpOnColumnSchemaInfo> _schemaMap;

    public CassandraColumnWrapper(IReadOnlyList<Row> parent, string columnName,
        IReadOnlyDictionary<string, NpOnColumnSchemaInfo> schemaMap) : base(parent)
    {
        _columnName = columnName;
        _schemaMap = schemaMap;
    }

    protected override IReadOnlyDictionary<int, INpOnCell> CreateResult()
    {
        var dictionary = new Dictionary<int, INpOnCell>();
        var schemaInfo = _schemaMap[_columnName];
        Type columnType = schemaInfo.DataType;
        Type genericCellType = typeof(NpOnCell<>).MakeGenericType(columnType);

        for (int i = 0; i < Parent.Count; i++)
        {
            Row row = Parent[i];
            INpOnCell cell = (INpOnCell)Activator.CreateInstance(
                genericCellType,
                row.GetValue(columnType, _columnName),
                columnType.ToDbType(),
                schemaInfo.ProviderDataTypeName
            )!;
            dictionary.Add(i, cell);
        }

        return dictionary;
    }

    public IReadOnlyDictionary<int, INpOnCell> GetColumnWrapper() => Result;
}

/// <summary>
/// Collection bọc Column -> truy cập theo cột/hàng 
/// </summary>
public class CassandraColumnCollection : IReadOnlyDictionary<string, CassandraColumnWrapper>,
    IReadOnlyDictionary<int, CassandraColumnWrapper>, INpOnCollectionWrapper
{
    private readonly List<CassandraColumnWrapper> _columnWrappers;
    private readonly IReadOnlyDictionary<string, int> _nameToIndexMap;

    public CassandraColumnCollection(IReadOnlyList<Row> allRows,
        IReadOnlyDictionary<string, NpOnColumnSchemaInfo> schemaMap)
    {
        var nameToIndexMap = new Dictionary<string, int>();
        _columnWrappers = new List<CassandraColumnWrapper>(schemaMap.Count);

        int i = 0;
        foreach (var schemaInfo in schemaMap.Values)
        {
            nameToIndexMap.Add(schemaInfo.ColumnName, i++);
            _columnWrappers.Add(new CassandraColumnWrapper(allRows, schemaInfo.ColumnName, schemaMap));
        }

        _nameToIndexMap = nameToIndexMap;
    }

    public CassandraColumnWrapper this[string columnName] => _columnWrappers[_nameToIndexMap[columnName]];
    public CassandraColumnWrapper this[int columnIndex] => _columnWrappers[columnIndex];

    // IReadOnlyDictionary...
    public IEnumerable<string> Keys => _nameToIndexMap.Keys;
    public IEnumerable<CassandraColumnWrapper> Values => _columnWrappers;
    public int Count => _columnWrappers.Count;
    public bool ContainsKey(string key) => _nameToIndexMap.ContainsKey(key);

    #region Implementation

    public bool TryGetValue(string key, out CassandraColumnWrapper value)
    {
        if (_nameToIndexMap.TryGetValue(key, out int index))
        {
            value = _columnWrappers[index];
            return true;
        }

        value = null!;
        return false;
    }

    public IEnumerator<KeyValuePair<string, CassandraColumnWrapper>> GetEnumerator()
    {
        foreach (var pair in _nameToIndexMap)
        {
            yield return new KeyValuePair<string, CassandraColumnWrapper>(pair.Key, _columnWrappers[pair.Value]);
        }
    }

    IEnumerable<int> IReadOnlyDictionary<int, CassandraColumnWrapper>.Keys => Enumerable.Range(0, Count);
    bool IReadOnlyDictionary<int, CassandraColumnWrapper>.ContainsKey(int key) => key >= 0 && key < Count;

    bool IReadOnlyDictionary<int, CassandraColumnWrapper>.TryGetValue(int key, out CassandraColumnWrapper value)
    {
        if (key >= 0 && key < Count)
        {
            value = _columnWrappers[key];
            return true;
        }

        value = null!;
        return false;
    }

    IEnumerator<KeyValuePair<int, CassandraColumnWrapper>> IEnumerable<KeyValuePair<int, CassandraColumnWrapper>>.
        GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return new KeyValuePair<int, CassandraColumnWrapper>(i, _columnWrappers[i]);
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    public IReadOnlyDictionary<int, INpOnColumnWrapper?> GetColumnWrapperByIndex(int[] indexes)
    {
        indexes = indexes.OrderByDescending(x => x).Where(x => x < Count).Distinct().ToArray();
        Dictionary<int, INpOnColumnWrapper?> result = new();
        foreach (var index in indexes)
            result.Add(index, _columnWrappers[index]);
        return result;
    }

    public IReadOnlyDictionary<string, INpOnColumnWrapper?> GetColumnWrapperByColumnName(string[] columnNames)
    {
        Dictionary<string, INpOnColumnWrapper?> result = new();
        foreach (var colName in columnNames)
            if (TryGetValue(colName, out var value))
                result.Add(colName, value);
        return result;
    }
}

/// <summary>
/// Data result of Query 
/// </summary>
public class CassandraResultSetWrapper : NpOnWrapperResult, INpOnTableWrapper
{
    private readonly IReadOnlyList<Row> _allRows;
    private readonly IReadOnlyDictionary<string, NpOnColumnSchemaInfo> _schemaMap;

    public IReadOnlyDictionary<int, CassandraRowWrapper> Rows { get; }
    public CassandraColumnCollection Columns { get; }

    public CassandraResultSetWrapper(RowSet? rowSet = null)
    {
        if (rowSet == null)
        {
            _allRows = [];
            _schemaMap = new Dictionary<string, NpOnColumnSchemaInfo>();
            Rows = new Dictionary<int, CassandraRowWrapper>();
            Columns = new CassandraColumnCollection(_allRows, _schemaMap);
            SetFail(EDbError.CassandraRowSetNull);
            return;
        }

        //  schema from Cql.Column
        var schemaMap = new Dictionary<string, NpOnColumnSchemaInfo>();
        foreach (var cqlColumn in rowSet.Columns)
        {
            var schemaInfo = new NpOnColumnSchemaInfo(
                cqlColumn.Name,
                cqlColumn.Type, // System.Type
                CassandraUtils.GetCqlTypeName(cqlColumn.Type)
            );
            schemaMap.Add(cqlColumn.Name, schemaInfo);
        }

        _schemaMap = schemaMap;
        // schema from Cql.Row
        _allRows = rowSet.ToList(); // Row -> access

        // View (Rows và Columns) <schemaMap> input
        Rows = _allRows
            .Select((row, index) => new { row, index })
            .ToDictionary(
                item => item.index,
                item => new CassandraRowWrapper(item.row, _schemaMap)
            );

        Columns = new CassandraColumnCollection(_allRows, _schemaMap);

        SetSuccess();
    }

    public IReadOnlyDictionary<int, INpOnRowWrapper?> RowWrappers
    {
        get
        {
            Dictionary<int, INpOnRowWrapper?> result = new();
            foreach (var row in Rows)
                result.Add(row.Key, row.Value);
            return result;
        }
    }

    public INpOnCollectionWrapper CollectionWrappers => Columns;
}