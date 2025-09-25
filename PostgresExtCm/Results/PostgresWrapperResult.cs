using System.Data;
using CommonDb.DbCommands;
using CommonDb.DbResults;
using Npgsql;

namespace PostgresExtCm.Results;



public class ColumnSchemaInfo
{
    public string ColumnName { get; }
    public Type DataType { get; }
    public string ProviderDataTypeName { get; } // Ví dụ: "text", "int4"

    public ColumnSchemaInfo(string columnName, Type dataType, string providerDataTypeName)
    {
        ColumnName = columnName;
        DataType = dataType;
        ProviderDataTypeName = providerDataTypeName;
    }
}


/// <summary>
/// ColumnWrapper
/// </summary>
public class PostgresRowWrapper : NpOnWrapperResult<DataRow, IReadOnlyDictionary<string, INpOnCell>>
{
    private readonly IReadOnlyDictionary<string, ColumnSchemaInfo> _schemaMap;

    public PostgresRowWrapper(DataRow parent, IReadOnlyDictionary<string, ColumnSchemaInfo> schemaMap) : base(parent)
    {
        _schemaMap = schemaMap;
    }

    protected override IReadOnlyDictionary<string, INpOnCell> CreateResult()
    {
        var dictionary = new Dictionary<string, INpOnCell>();

        foreach (var schemaInfo in _schemaMap.Values)
        {
            object cellValue = Parent[schemaInfo.ColumnName];
            Type columnType = schemaInfo.DataType;

            Type genericCellType = typeof(NpOnCell<>).MakeGenericType(columnType);

            INpOnCell cell = (INpOnCell)Activator.CreateInstance(
                genericCellType,
                cellValue,
                columnType.ToDbType(),
                schemaInfo.ProviderDataTypeName // SỬ DỤNG THÔNG TIN CHÍNH XÁC TỪ SCHEMA
            )!;

            dictionary.Add(schemaInfo.ColumnName, cell);
        }
        return dictionary;
    }
}



/// <summary>
/// ColumnWrapper (truy cập được từ Key-integer hoặc Key-string)
/// </summary>
public class PostgresColumnWrapper : NpOnWrapperResult<DataTable, IReadOnlyDictionary<int, INpOnCell>>
{
    private readonly string _columnName;
    private readonly IReadOnlyDictionary<string, ColumnSchemaInfo> _schemaMap;

    public PostgresColumnWrapper(DataTable parent, string columnName, IReadOnlyDictionary<string, ColumnSchemaInfo> schemaMap) : base(parent)
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

        for (int i = 0; i < Parent.Rows.Count; i++)
        {
            DataRow row = Parent.Rows[i];
            INpOnCell cell = (INpOnCell)Activator.CreateInstance(
                genericCellType,
                row[_columnName],
                columnType.ToDbType(),
                schemaInfo.ProviderDataTypeName // SỬ DỤNG THÔNG TIN CHÍNH XÁC TỪ SCHEMA
            )!;
            dictionary.Add(i, cell);
        }
        return dictionary;
    }
}



/// Collection bọc Column -> truy cập theo cột/hàng 
/// <summary>
/// </summary>
public class PostgresColumnCollection : IReadOnlyDictionary<string, PostgresColumnWrapper>, IReadOnlyDictionary<int, PostgresColumnWrapper>
{
    private readonly List<PostgresColumnWrapper> _columnWrappers;
    private readonly IReadOnlyDictionary<string, int> _nameToIndexMap;

    public PostgresColumnCollection(DataTable dataTable, IReadOnlyDictionary<string, ColumnSchemaInfo> schemaMap)
    {
        var nameToIndexMap = new Dictionary<string, int>();
        _columnWrappers = new List<PostgresColumnWrapper>(dataTable.Columns.Count);

        int i = 0;
        foreach (var schemaInfo in schemaMap.Values)
        {
            nameToIndexMap.Add(schemaInfo.ColumnName, i++);
            _columnWrappers.Add(new PostgresColumnWrapper(dataTable, schemaInfo.ColumnName, schemaMap));
        }
        _nameToIndexMap = nameToIndexMap;
    }

    public PostgresColumnWrapper this[string columnName] => _columnWrappers[_nameToIndexMap[columnName]];
    public PostgresColumnWrapper this[int columnIndex] => _columnWrappers[columnIndex];
    
    // reader
    public IEnumerable<string> Keys => _nameToIndexMap.Keys;
    public IEnumerable<PostgresColumnWrapper> Values => _columnWrappers;
    public int Count => _columnWrappers.Count;
    public bool ContainsKey(string key) => _nameToIndexMap.ContainsKey(key);

    public bool TryGetValue(string key, out PostgresColumnWrapper value)
    {
        if (_nameToIndexMap.TryGetValue(key, out int index))
        {
            value = _columnWrappers[index];
            return true;
        }
        value = null!;
        return false;
    }

    public IEnumerator<KeyValuePair<string, PostgresColumnWrapper>> GetEnumerator()
    {
        foreach (var pair in _nameToIndexMap)
        {
            yield return new KeyValuePair<string, PostgresColumnWrapper>(pair.Key, _columnWrappers[pair.Value]);
        }
    }

    // Triển khai cho IReadOnlyDictionary<int, ...>
    IEnumerable<int> IReadOnlyDictionary<int, PostgresColumnWrapper>.Keys => Enumerable.Range(0, Count);
    bool IReadOnlyDictionary<int, PostgresColumnWrapper>.ContainsKey(int key) => key >= 0 && key < Count;

    bool IReadOnlyDictionary<int, PostgresColumnWrapper>.TryGetValue(int key, out PostgresColumnWrapper value)
    {
        if (key >= 0 && key < Count)
        {
            value = _columnWrappers[key];
            return true;
        }

        value = null!;
        return false;
    }

    IEnumerator<KeyValuePair<int, PostgresColumnWrapper>> IEnumerable<KeyValuePair<int, PostgresColumnWrapper>>.
        GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return new KeyValuePair<int, PostgresColumnWrapper>(i, _columnWrappers[i]);
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

public class PostgresResultSetWrapper
{
    private readonly DataTable _dataTable;
    private readonly IReadOnlyDictionary<string, ColumnSchemaInfo> _schemaMap;

    public IReadOnlyDictionary<int, PostgresRowWrapper> Rows { get; }
    public PostgresColumnCollection Columns { get; }

    public PostgresResultSetWrapper(NpgsqlDataReader reader)
    {
        // schema 
        var schemaMap = new Dictionary<string, ColumnSchemaInfo>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            var schemaInfo = new ColumnSchemaInfo(
                columnName,
                reader.GetFieldType(i), // LSystem.Type
                reader.GetDataTypeName(i) // POSTGRES 
            );
            schemaMap.Add(columnName, schemaInfo);
        }
        _schemaMap = schemaMap;
        _dataTable = new DataTable();
        _dataTable.Load(reader); 

        Rows = _dataTable.Rows
            .Cast<DataRow>()
            .Select((row, index) => new { row, index })
            .ToDictionary(
                item => item.index,
                item => new PostgresRowWrapper(item.row, _schemaMap) // Truyền schema vào
            );

        Columns = new PostgresColumnCollection(_dataTable, _schemaMap); // Truyền schema vào
    }
}