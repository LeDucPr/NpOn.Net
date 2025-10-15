using System.Data;
using CommonDb;
using CommonDb.DbResults;
using Enums;
using Microsoft.Data.SqlClient;

namespace MssqlExtCm.Results;

/// <summary>
/// ColumnWrapper
/// </summary>
public class MssqlRowWrapper : NpOnWrapperResult<DataRow, IReadOnlyDictionary<string, INpOnCell>>
{
    private readonly IReadOnlyDictionary<string, NpOnColumnSchemaInfo> _schemaMap;

    public MssqlRowWrapper(DataRow parent, IReadOnlyDictionary<string, NpOnColumnSchemaInfo> schemaMap) :
        base(parent)
    {
        _schemaMap = schemaMap;
    }

    protected override IReadOnlyDictionary<string, INpOnCell> CreateResult()
    {
        var dictionary = new Dictionary<string, INpOnCell>();

        foreach (var schemaInfo in _schemaMap.Values)
        {
            object? cellValue = Parent[schemaInfo.ColumnName];
            Type columnType = schemaInfo.DataType;
            Type genericCellType = typeof(NpOnCell<>).MakeGenericType(columnType);

            if (cellValue == DBNull.Value)
                cellValue = null;

            INpOnCell cell = (INpOnCell)Activator.CreateInstance(
                genericCellType,
                cellValue,
                columnType.ToDbType(),
                schemaInfo.ProviderDataTypeName // THÔNG TIN CHÍNH XÁC SCHEMA
            )!;

            dictionary.Add(schemaInfo.ColumnName, cell);
        }

        return dictionary;
    }
}

/// <summary>
/// ColumnWrapper (truy cập được từ Key-integer hoặc Key-string)
/// </summary>
public class MssqlColumnWrapper : NpOnWrapperResult<DataTable, IReadOnlyDictionary<int, INpOnCell>>
{
    private readonly string _columnName;
    private readonly IReadOnlyDictionary<string, NpOnColumnSchemaInfo> _schemaMap;

    public MssqlColumnWrapper(DataTable parent, string columnName,
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

/// <summary>
/// Collection bọc Column -> truy cập theo cột/hàng 
/// </summary>
public class MssqlColumnCollection : IReadOnlyDictionary<string, MssqlColumnWrapper>,
    IReadOnlyDictionary<int, MssqlColumnWrapper>
{
    private readonly List<MssqlColumnWrapper> _columnWrappers;
    private readonly IReadOnlyDictionary<string, int> _nameToIndexMap;

    public MssqlColumnCollection(DataTable dataTable, IReadOnlyDictionary<string, NpOnColumnSchemaInfo> schemaMap)
    {
        var nameToIndexMap = new Dictionary<string, int>();
        _columnWrappers = new List<MssqlColumnWrapper>(dataTable.Columns.Count);

        int i = 0;
        foreach (var schemaInfo in schemaMap.Values)
        {
            nameToIndexMap.Add(schemaInfo.ColumnName, i++);
            _columnWrappers.Add(new MssqlColumnWrapper(dataTable, schemaInfo.ColumnName, schemaMap));
        }

        _nameToIndexMap = nameToIndexMap;
    }

    public MssqlColumnWrapper this[string columnName] => _columnWrappers[_nameToIndexMap[columnName]];
    public MssqlColumnWrapper this[int columnIndex] => _columnWrappers[columnIndex];

    // reader
    public IEnumerable<string> Keys => _nameToIndexMap.Keys;
    public IEnumerable<MssqlColumnWrapper> Values => _columnWrappers;
    public int Count => _columnWrappers.Count;
    public bool ContainsKey(string key) => _nameToIndexMap.ContainsKey(key);

    public bool TryGetValue(string key, out MssqlColumnWrapper value)
    {
        if (_nameToIndexMap.TryGetValue(key, out int index))
        {
            value = _columnWrappers[index];
            return true;
        }

        value = null!;
        return false;
    }

    public IEnumerator<KeyValuePair<string, MssqlColumnWrapper>> GetEnumerator()
    {
        foreach (var pair in _nameToIndexMap)
        {
            yield return new KeyValuePair<string, MssqlColumnWrapper>(pair.Key, _columnWrappers[pair.Value]);
        }
    }

    // IReadOnlyDictionary<int, ...>
    IEnumerable<int> IReadOnlyDictionary<int, MssqlColumnWrapper>.Keys => Enumerable.Range(0, Count);
    bool IReadOnlyDictionary<int, MssqlColumnWrapper>.ContainsKey(int key) => key >= 0 && key < Count;

    bool IReadOnlyDictionary<int, MssqlColumnWrapper>.TryGetValue(int key, out MssqlColumnWrapper value)
    {
        if (key >= 0 && key < Count)
        {
            value = _columnWrappers[key];
            return true;
        }

        value = null!;
        return false;
    }

    IEnumerator<KeyValuePair<int, MssqlColumnWrapper>> IEnumerable<KeyValuePair<int, MssqlColumnWrapper>>.
        GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return new KeyValuePair<int, MssqlColumnWrapper>(i, _columnWrappers[i]);
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

public class MssqlResultSetWrapper : NpOnWrapperResult
{
    private readonly DataTable _dataTable;
    private readonly IReadOnlyDictionary<string, NpOnColumnSchemaInfo> _schemaMap;

    public IReadOnlyDictionary<int, MssqlRowWrapper> Rows { get; }
    public MssqlColumnCollection Columns { get; }

    public MssqlResultSetWrapper(SqlDataReader? reader = null)
    {
        // schema 
        if (reader == null)
        {
            SetFail(EDbError.MssqlDataTableNull);
            return;
        }

        var schemaMap = new Dictionary<string, NpOnColumnSchemaInfo>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            var schemaInfo = new NpOnColumnSchemaInfo(
                columnName,
                reader.GetFieldType(i), // LSystem.Type
                reader.GetDataTypeName(i) // MSSQL 
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
                item => new MssqlRowWrapper(item.row, _schemaMap) // schema -> Row
            );

        Columns = new MssqlColumnCollection(_dataTable, _schemaMap); // schema -> Column
        SetSuccess();
    }
}