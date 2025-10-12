using HandlerFlow.AlgObjs.CtrlObjs;
using CommonMode;
using HandlerFlow.AlgObjs.Attributes;
using HandlerFlow.AlgObjs.RaisingRouters;

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

    // self value
    private object? _idValue = null;
    private Type? _idValueType = null;
    private string? _idName = null;
    
    /// <summary>
    /// bao gồm cả việc lấy giá trị khóa chính (single) 
    /// </summary>
    /// <param name="ctrl"></param>
    public BaseQueryCreatorWithKey(BaseCtrl ctrl)
    {
        if (!ctrl.GetType().IsChildOfBaseCtrl())
            return;
        TableLoaderAttribute? tableLoaderAttr = ctrl.GetType().GetClassAttribute<TableLoaderAttribute>();
        if (tableLoaderAttr == null)
            return;
        _table = tableLoaderAttr.TableName.ToLower();
        _baseQuery += _table; // query get all
        
        var pkInfo = ctrl.PrimaryKeys()?.FirstOrDefault();
        if (pkInfo == null)
            return;

        _idName = pkInfo.Property.Name;
        _idValue = pkInfo.Property.GetValue(ctrl);
        _idValueType = pkInfo.Property.PropertyType;
    }

    protected virtual string CreateOperation(List<(string Id, object? Value, Type ValueType)> keyValues,
        string? baseInput = null)
    {
        string output = baseInput ?? _baseQuery;
        output += output.TrimEnd().EndsWith(_whereClause.Trim()) ? _andClause : _whereClause;
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

    public virtual string CreateQueryWithId()
    {
        if (_idName == null || _idValue == null || _idValueType == null)
            return string.Empty;
        return CreateOperation([(_idName, _idValue, _idValueType)]);
    }
}