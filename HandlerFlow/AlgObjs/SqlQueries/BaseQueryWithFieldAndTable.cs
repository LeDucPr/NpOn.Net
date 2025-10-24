using Enums;
using System.Text;
using HandlerFlow.AlgObjs.CtrlObjs.Data;

namespace HandlerFlow.AlgObjs.SqlQueries;

public class BaseQueryWithFieldAndTable
{
    private readonly string _table;
    private EDb _dbType;

    // For single query
    private readonly (string Name, object? Value, Type ValueType)? _singlePk;

    // For bulk query
    private readonly (string Name, List<object?> Values, Type ValueType)? _bulkPks;

    // validate 
    private void Validate(EDb? dbType)
    {
        if (dbType == null)
            throw new NotSupportedException($"Database type not null.");
        _dbType = (EDb)dbType;
        if (_dbType != EDb.Postgres && _dbType != EDb.Mssql && _dbType != EDb.Cassandra && _dbType != EDb.ScyllaDb)
            throw new NotSupportedException($"Database type '{dbType}' is not supported.");
    }

    private BaseQueryWithFieldAndTable(List<UnifiedTableMappingCtrl> fieldMappings, EDb dbType)
    {
        if (fieldMappings is not { Count : > 0 })
            throw new ArgumentException("Field not null");

        bool isInATable = !(fieldMappings.Select(x => x.JoinTableField?.Table?.Id).Distinct().Count() > 1);
        if (!isInATable)
            return;

        Validate(fieldMappings.First().TableField?.Table?.ConnectionInfo?.DatabaseType);
        TableCtrl table = fieldMappings.First().TableField?.Table ?? throw new ArgumentException("Table not null");
        _table = table.TableName;
        // _singlePk = singlePk; //////////////////////////////////////////////////////////
        _bulkPks = null;
        _dbType = dbType;
    }

    // từ hàm overload trên, tạo một đối tượng trả ra kết quả của câu truy vấn có chứa bảng và các trường được truy vấn dang static    public static BaseQueryWithFieldAndTable FromFieldMappings(List<UnifiedTableMappingCtrl> fieldMappings)
    public static string CreateQuery(List<UnifiedTableMappingCtrl> fieldMappings, EDb dbType)
    {
        var query = new BaseQueryWithFieldAndTable(fieldMappings, dbType);
        return query.BuildSelectQuery(fieldMappings.Select(x => x.TableField?.FieldName!).Distinct().ToList());
    }


    /// <summary>
    /// Constructor for a query targeting a single entity by its primary key.
    /// </summary>
    public BaseQueryWithFieldAndTable(string table, EDb dbType, (string Name, object? Value, Type ValueType) singlePk)
    {
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("Table name cannot be null or whitespace.", nameof(table));
        if (string.IsNullOrWhiteSpace(singlePk.Name))
            throw new ArgumentException("Primary key name cannot be null or whitespace.", nameof(singlePk));

        Validate(dbType);
        _table = table;
        _singlePk = singlePk;
        _bulkPks = null;
    }

    /// <summary>
    /// Constructor for a query targeting multiple entities by a list of primary key values.
    /// </summary>
    public BaseQueryWithFieldAndTable(string table, EDb dbType,
        (string Name, List<object?> Values, Type ValueType) bulkPks)
    {
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("Table name cannot be null or whitespace.", nameof(table));
        if (string.IsNullOrWhiteSpace(bulkPks.Name))
            throw new ArgumentException("Primary key name cannot be null or whitespace.", nameof(bulkPks));
        if (bulkPks.Values is not { Count: > 0 })
            throw new ArgumentException("Primary key values list cannot be null or empty.", nameof(bulkPks));

        Validate(dbType);
        _table = table;
        _bulkPks = bulkPks;
        _singlePk = null;
    }

    /// <summary>
    /// Builds the SELECT query string based on the provided fields.
    /// </summary>
    /// <param name="fields">A list of column names to select.</param>
    /// <param name="condition"></param>
    /// <returns>A complete SQL SELECT statement.</returns>
    public string BuildSelectQuery(List<string> fields, string? condition = null)
    {
        if (fields is not { Count: > 0 })
            throw new ArgumentException("Fields list cannot be null or empty.", nameof(fields));

        var queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT ").Append(string.Join(", ", fields));
        queryBuilder.Append(" FROM ").Append(_table);
        if (condition != null)
            queryBuilder.Append(" WHERE ").Append(condition);
        if (_singlePk.HasValue) // = 
        {
            var pk = _singlePk.Value;
            queryBuilder.Append(pk.Name)
                .Append(" = ")
                .Append(FormatValue(pk.Value, pk.ValueType));
        }
        else if (_bulkPks.HasValue) // in 
        {
            var pks = _bulkPks.Value;
            var formattedValues = pks.Values.Select(v => FormatValue(v, pks.ValueType));
            queryBuilder.Append(pks.Name)
                .Append(" IN (")
                .Append(string.Join(", ", formattedValues))
                .Append(')');
        }

        return queryBuilder.ToString();
    }

    private static string FormatValue(object? value, Type valueType)
    {
        if (value == null) return "NULL";
        if (valueType == typeof(int) || valueType == typeof(long) || valueType == typeof(decimal) ||
            valueType == typeof(double) || valueType == typeof(float))
            return value.ToString()!;
        return $"'{value.ToString()!.Replace("'", "''")}'";
    }
}