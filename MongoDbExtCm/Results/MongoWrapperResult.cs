using CommonDb.DbResults;
using Enums;
using MongoDB.Bson;

namespace MongoDbExtCm.Results;

/// <summary>
/// Schema info of Field/Column in MongoDB.
/// </summary>
public class MongoColumnInfo
{
    public string ColumnName { get; }
    public Type DotNetType { get; }
    public string BsonTypeName { get; }
    public BsonType BsonType { get; }

    public MongoColumnInfo(string columnName, Type dotNetType, string bsonTypeName, BsonType bsonType)
    {
        ColumnName = columnName;
        DotNetType = dotNetType;
        BsonTypeName = bsonTypeName;
        BsonType = bsonType;
    }
}

/// <summary>
/// Wrapper (Row/BsonDocument) of MongoDB
/// </summary>
public class MongoRowWrapper : NpOnWrapperResult<BsonDocument, IReadOnlyDictionary<string, INpOnCell>>, INpOnRowWrapper
{
    private readonly IReadOnlyDictionary<string, MongoColumnInfo> _schemaMap;

    public MongoRowWrapper(BsonDocument parent, IReadOnlyDictionary<string, MongoColumnInfo> schemaMap) : base(parent)
    {
        _schemaMap = schemaMap;
    }

    protected override IReadOnlyDictionary<string, INpOnCell> CreateResult()
    {
        var dictionary = new Dictionary<string, INpOnCell>();

        foreach (var schemaInfo in _schemaMap.Values)
        {
            Parent.TryGetValue(schemaInfo.ColumnName, out var bsonValue);

            if (bsonValue == null)
            {
                var nullCellType = typeof(NpOnCell<>).MakeGenericType(schemaInfo.DotNetType);
                var nullCell = (INpOnCell)Activator.CreateInstance(nullCellType,
                    new object?[] { null, schemaInfo.DotNetType.ToDbType(), schemaInfo.BsonTypeName })!;
                dictionary.Add(schemaInfo.ColumnName, nullCell);
                continue;
            }

            object? cellValue;
            if (bsonValue.IsBsonDocument)
            {
                cellValue = new MongoResultSetWrapper([bsonValue.AsBsonDocument]);
            }
            else if (bsonValue.IsBsonArray)
            {
                var documentsInArray = bsonValue.AsBsonArray
                    .Where(v => v.IsBsonDocument)
                    .Select(v => v.AsBsonDocument)
                    .ToList();
                cellValue = new MongoResultSetWrapper(documentsInArray);
            }
            else
            {
                cellValue = BsonTypeMapper.MapToDotNetValue(bsonValue);
            }

            var genericCellType = typeof(NpOnCell<>).MakeGenericType(schemaInfo.DotNetType);
            var cell = (INpOnCell)Activator.CreateInstance(genericCellType,
                new[] { cellValue, schemaInfo.DotNetType.ToDbType(), schemaInfo.BsonTypeName })!;
            dictionary.Add(schemaInfo.ColumnName, cell);
        }

        return dictionary;
    }

    public IReadOnlyDictionary<string, INpOnCell> GetRowWrapper() => Result;
}

/// <summary>
/// Wrapper (Column/Field) of MongoDB.
/// </summary>
public class MongoColumnWrapper : NpOnWrapperResult<IReadOnlyList<BsonDocument>, IReadOnlyDictionary<int, INpOnCell>>,
    INpOnColumnWrapper
{
    private readonly string _columnName;
    private readonly IReadOnlyDictionary<string, MongoColumnInfo> _schemaMap;

    public MongoColumnWrapper(IReadOnlyList<BsonDocument> parent, string columnName,
        IReadOnlyDictionary<string, MongoColumnInfo> schemaMap) : base(parent)
    {
        _columnName = columnName;
        _schemaMap = schemaMap;
    }

    protected override IReadOnlyDictionary<int, INpOnCell> CreateResult()
    {
        var dictionary = new Dictionary<int, INpOnCell>();

        for (int i = 0; i < Parent.Count; i++)
        {
            var rowDocument = Parent[i];
            var rowWrapper = new MongoRowWrapper(rowDocument, _schemaMap);
            var cell = rowWrapper.Result[_columnName];
            dictionary.Add(i, cell);
        }

        return dictionary;
    }

    public IReadOnlyDictionary<int, INpOnCell> GetColumnWrapper() => Result;
}

/// <summary>
/// Custom MongoDB.
/// </summary>
public class MongoColumnCollection : IReadOnlyDictionary<string, MongoColumnWrapper>,
    IReadOnlyDictionary<int, MongoColumnWrapper>, INpOnCollectionWrapper
{
    private readonly List<MongoColumnWrapper> _columnWrappers;
    private readonly IReadOnlyDictionary<string, int> _nameToIndexMap;

    public MongoColumnCollection(IReadOnlyList<BsonDocument> allRows,
        IReadOnlyDictionary<string, MongoColumnInfo> schemaMap)
    {
        var nameToIndexMap = new Dictionary<string, int>();
        _columnWrappers = new List<MongoColumnWrapper>(schemaMap.Count);

        int i = 0;
        foreach (var schemaInfo in schemaMap.Values)
        {
            nameToIndexMap.Add(schemaInfo.ColumnName, i++);
            _columnWrappers.Add(new MongoColumnWrapper(allRows, schemaInfo.ColumnName, schemaMap));
        }

        _nameToIndexMap = nameToIndexMap;
    }

    // --- Implementation ---

    public MongoColumnWrapper this[string columnName] => _columnWrappers[_nameToIndexMap[columnName]];
    public MongoColumnWrapper this[int columnIndex] => _columnWrappers[columnIndex];

    public IEnumerable<string> Keys => _nameToIndexMap.Keys;
    public IEnumerable<MongoColumnWrapper> Values => _columnWrappers;
    public int Count => _columnWrappers.Count;
    public bool ContainsKey(string key) => _nameToIndexMap.ContainsKey(key);

    public bool TryGetValue(string key, out MongoColumnWrapper value)
    {
        if (_nameToIndexMap.TryGetValue(key, out int index))
        {
            value = _columnWrappers[index];
            return true;
        }

        value = null!;
        return false;
    }

    public IEnumerator<KeyValuePair<string, MongoColumnWrapper>> GetEnumerator()
    {
        foreach (var pair in _nameToIndexMap)
        {
            yield return new KeyValuePair<string, MongoColumnWrapper>(pair.Key, _columnWrappers[pair.Value]);
        }
    }

    IEnumerable<int> IReadOnlyDictionary<int, MongoColumnWrapper>.Keys => Enumerable.Range(0, Count);

    bool IReadOnlyDictionary<int, MongoColumnWrapper>.ContainsKey(int key) => key >= 0 && key < Count;

    bool IReadOnlyDictionary<int, MongoColumnWrapper>.TryGetValue(int key, out MongoColumnWrapper value)
    {
        if (key >= 0 && key < Count)
        {
            value = _columnWrappers[key];
            return true;
        }

        value = null!;
        return false;
    }

    IEnumerator<KeyValuePair<int, MongoColumnWrapper>> IEnumerable<KeyValuePair<int, MongoColumnWrapper>>.
        GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return new KeyValuePair<int, MongoColumnWrapper>(i, _columnWrappers[i]);
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

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
/// BsonDocument
/// </summary>
public class MongoResultSetWrapper : NpOnWrapperResult, INpOnTableWrapper
{
    private readonly IReadOnlyList<BsonDocument> _allRows;
    private readonly IReadOnlyDictionary<string, MongoColumnInfo> _schemaMap;

    public IReadOnlyDictionary<int, MongoRowWrapper> Rows { get; }
    public MongoColumnCollection Columns { get; }

    public MongoResultSetWrapper(List<BsonDocument>? documents = null)
    {
        if (documents == null)
        {
            SetFail(EDbError.MongoDbBsonDocumentNull);
            return;
        }

        _allRows = new List<BsonDocument>();

        var schemaMap = new Dictionary<string, MongoColumnInfo>();
        var allKeys = _allRows.SelectMany(doc => doc.Names).Distinct();

        foreach (var key in allKeys)
        {
            var sampleValue = _allRows.FirstOrDefault(doc => doc.Contains(key))?[key];
            var bsonType = sampleValue?.BsonType ?? BsonType.Null;

            var schemaInfo = new MongoColumnInfo(
                key,
                MongoDbUtils.GetDotNetType(bsonType),
                MongoDbUtils.GetBsonTypeName(bsonType),
                bsonType
            );
            schemaMap.Add(key, schemaInfo);
        }

        _schemaMap = schemaMap;

        Rows = _allRows
            .Select((doc, index) => new { doc, index })
            .ToDictionary(
                item => item.index,
                item => new MongoRowWrapper(item.doc, _schemaMap)
            );

        Columns = new MongoColumnCollection(_allRows, _schemaMap);
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