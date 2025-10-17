using System.Collections;
using System.Data;
using CommonDb;
using CommonDb.DbResults;
using Enums;
using Microsoft.Data.SqlClient;

namespace MssqlExtCm.Results;

/// <summary>
/// ColumnWrapper
/// </summary>
public class MssqlRowWrapper : NpOnWrapperResult<DataRow, IReadOnlyDictionary<string, INpOnCell>>, INpOnRowWrapper
{
    private readonly IReadOnlyDictionary<string, NpOnColumnSchemaInfo> _schemaMap;

    public MssqlRowWrapper(DataRow parent, IReadOnlyDictionary<string, NpOnColumnSchemaInfo> schemaMap) :
        base(parent)
    {
        _schemaMap = schemaMap;
    }

    protected override IReadOnlyDictionary<string, INpOnCell> CreateResult()
    {
        Dictionary<string, INpOnCell> dictionary = new Dictionary<string, INpOnCell>();
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

    public IReadOnlyDictionary<string, INpOnCell> GetRowWrapper() => CreateResult();
}

/// <summary>
/// ColumnWrapper (truy cập được từ Key-integer hoặc Key-string)
/// </summary>
public class MssqlColumnWrapper : NpOnWrapperResult<DataTable, IReadOnlyDictionary<int, INpOnCell>>, INpOnColumnWrapper
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
        Dictionary<int, INpOnCell> dictionary = new Dictionary<int, INpOnCell>();
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

    public IReadOnlyDictionary<int, INpOnCell> GetColumnWrapper() => CreateResult();
}

/// <summary>
/// Collection bọc Column -> truy cập theo cột/hàng 
/// </summary>
public class MssqlColumnCollection : IReadOnlyDictionary<string, MssqlColumnWrapper>,
    IReadOnlyDictionary<int, MssqlColumnWrapper>, INpOnCollectionWrapper
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

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IReadOnlyDictionary<int, INpOnColumnWrapper?> GetColumnWrapperByIndexes(int[] indexes)
    {
        indexes = indexes.OrderByDescending(x => x).Where(x => x < Count).Distinct().ToArray();
        Dictionary<int, INpOnColumnWrapper?> result = new();
        foreach (var index in indexes)
            result.Add(index, _columnWrappers[index]);
        return result;
    }

    public IReadOnlyDictionary<string, INpOnColumnWrapper?> GetColumnWrapperByColumnNames(string[]? columnNames = null)
    {
        columnNames ??= Keys.ToArray(); // get all
        Dictionary<string, INpOnColumnWrapper?> result = new();
        foreach (var colName in columnNames)
            if (TryGetValue(colName, out var value))
                result.Add(colName, value);
        return result;
    }
}

public class MssqlResultSetWrapper : NpOnWrapperResult, INpOnTableWrapper
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