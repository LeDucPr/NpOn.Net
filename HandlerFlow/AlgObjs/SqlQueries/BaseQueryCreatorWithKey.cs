using HandlerFlow.AlgObjs.CtrlObjs;
using CommonMode;
using Enums;
using HandlerFlow.AlgObjs.Attributes;
using HandlerFlow.AlgObjs.RaisingRouters;

namespace HandlerFlow.AlgObjs.SqlQueries;

public sealed class BaseQueryCreatorWithKey
{
    private readonly string _table;
    private EDb _dbType;
    private readonly string _baseQuery = "select * from ";
    private readonly string _whereClause = " where ";
    private readonly string _andClause = " and ";
    private readonly string _equalClause = " = ";
    private readonly string _apostrophe = "'";
    private readonly string _apostropheEmpty = "''";

    // For single query
    private readonly (string Name, object? Value, Type ValueType)? _singlePk;

    // For bulk query
    private readonly (string Name, List<object?> Values, Type ValueType)? _bulkPks;

    // validate 
    private bool Validate(EDb dbType)
    {
        _dbType = dbType;
        return _dbType == EDb.Postgres || _dbType == EDb.Mssql;
    }

    /// <summary>
    /// Constructor for create query for one object (single query).
    /// </summary>
    /// <param name="ctrl"></param>
    /// <exception cref="NotSupportedException"></exception>
    public BaseQueryCreatorWithKey(BaseCtrl ctrl)
    {
        if (!ctrl.GetType().IsChildOfBaseCtrl())
            throw new ArgumentException("Control object must be a derivative of BaseCtrl.", nameof(ctrl));
        TableLoaderAttribute? tableLoaderAttr = ctrl.GetType().GetClassAttribute<TableLoaderAttribute>();
        if (tableLoaderAttr == null)
            throw new InvalidOperationException("Control object is missing the TableLoaderAttribute.");

        _table = tableLoaderAttr.TableName.ToLower();
        _baseQuery += _table; // query get all

        var pkInfo = ctrl.PrimaryKeys()?.FirstOrDefault();
        if (pkInfo == null)
            throw new InvalidOperationException("Control object does not have a primary key defined.");

        _singlePk = (pkInfo.Property.Name, pkInfo.Property.GetValue(ctrl), pkInfo.Property.PropertyType);
    }

    /// <summary>
    /// Constructor to create query for List object (bulk query).
    /// </summary>
    /// <param name="ctrls"></param>
    /// <exception cref="NotSupportedException"></exception>
    public BaseQueryCreatorWithKey(List<BaseCtrl> ctrls)
    {
        if (ctrls is not { Count: > 0 } ||
            ctrls.Select(t => t.GetType()).Distinct().ToArray() is not { Length: 1 })
            throw new ArgumentException("Control list cannot be null or empty.", nameof(ctrls));

        var firstCtrl = ctrls.First(); // first to get type
        TableLoaderAttribute? tableLoaderAttr = firstCtrl.GetType().GetClassAttribute<TableLoaderAttribute>();
        if (tableLoaderAttr == null)
            throw new InvalidOperationException("Control objects are missing the TableLoaderAttribute.");

        _table = tableLoaderAttr.TableName.ToLower();
        _baseQuery += _table;

        var pkInfo = firstCtrl.PrimaryKeys()?.FirstOrDefault();
        if (pkInfo == null)
            throw new InvalidOperationException("Control objects do not have a primary key defined.");

        var pkValues = ctrls.Select(c => pkInfo.Property.GetValue(c)).ToList();
        _bulkPks = (pkInfo.Property.Name, pkValues, pkInfo.Property.PropertyType);
    }

    private string CreateOperation(
        List<(string Id, object? Value, Type ValueType)> keyValues,
        EDb dbType = EDb.Postgres,
        string? baseInput = null)
    {
        if (!Validate(dbType))
            return string.Empty;
        string output = baseInput ?? _baseQuery;
        bool hasWhere = output.Contains(_whereClause, StringComparison.OrdinalIgnoreCase);
        output += hasWhere ? _andClause : _whereClause;

        foreach (var keyValue in keyValues)
        {
            string value = keyValue.Value?.ToString() ?? _apostropheEmpty;
            if (keyValue.ValueType != typeof(int) && keyValue.ValueType != typeof(decimal) &&
                keyValue.ValueType != typeof(float) && keyValue.ValueType != typeof(double))
                value = _apostrophe + value + _apostrophe;
            output += keyValue.Id + _equalClause + value;
        }

        return output;
    }

    public string CreateQueryWithId(EDb dbType)
    {
        if (_singlePk == null /*|| _singlePk.Value.Name == null*/ || _singlePk.Value.Value == null)
            return string.Empty;
        return CreateOperation(
            [(_singlePk.Value.Name, _singlePk.Value.Value, _singlePk.Value.ValueType)], dbType);
    }

    public string CreateQueryWithIds(EDb dbType)
    {
        if (!Validate(dbType))
            return string.Empty;
        if (_bulkPks == null || _bulkPks.Value.Values.Count == 0)
            return string.Empty;

        var pkName = _bulkPks.Value.Name;
        var pkType = _bulkPks.Value.ValueType;
        var pkValues = _bulkPks.Value.Values.Where(v => v != null).ToList();

        if (pkValues.Count == 0)
            return string.Empty;
        
        if (pkValues.Count == 1)
            return CreateOperation([(pkName, pkValues.First(), pkType)], dbType);

        var formattedValues = pkValues.Select(v => FormatValue(v, pkType));
        string inClause = string.Join(", ", formattedValues);

        return $"{_baseQuery}{_whereClause}{pkName} IN ({inClause})";
    }

    private string FormatValue(object? value, Type valueType)
    {
        if (value == null) return "NULL";

        if (valueType == typeof(string) || valueType == typeof(Guid) || valueType == typeof(DateTime))
            return $"{_apostrophe}{value}{_apostrophe}";

        return value.ToString()!;
    }
}