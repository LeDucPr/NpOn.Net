using HandlerFlow.AlgObjs.CtrlObjs;
using CommonMode;
using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.SqlQueries;

public class BaseQueryCreatorWithKey
{
    private readonly string _table;
    private readonly string _baseQuery = "select * from ";
    private readonly string _whereClause = " where ";
    private readonly string _andClause = " and ";
    private readonly string _equalClause = " = ";
    private readonly string _apostrophe = "'";
    private readonly string _apostropheEmpty = "''";

    public BaseQueryCreatorWithKey(BaseCtrl ctrl)
    {
        TableLoaderAttribute? tableLoaderAttr = ctrl.GetType().GetClassAttribute<TableLoaderAttribute>();
        if (tableLoaderAttr == null)
            return;
        _table = tableLoaderAttr.TableName.ToLower();
        _baseQuery += _table; // query get all
    }

    protected string CreateOperation(List<(string Id, object? Value, Type ValueType)> keyValues,
        string? baseInput = null)
    {
        string output = baseInput ?? _baseQuery;
        output += output.TrimEnd().EndsWith(_whereClause.Trim()) ? _andClause : _whereClause;
        foreach (var keyValue in keyValues)
        {
            string value = keyValue.Value?.ToString() ?? _apostropheEmpty;
            if (keyValue.ValueType != typeof(int) || keyValue.ValueType != typeof(decimal) ||
                keyValue.ValueType != typeof(float) || keyValue.ValueType != typeof(double))
                value = _apostrophe + value + _apostrophe;
            output += keyValue.Id + _equalClause + value;
        }

        return output;
    }

    public virtual string QueryWithId(string id, object? value, Type valueType)
    {
        return CreateOperation([(id, value, valueType)]);
    }
}