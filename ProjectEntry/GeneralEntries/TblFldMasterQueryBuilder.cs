using System.Text;

namespace ProjectEntry.GeneralEntries;

public class TblFldMasterQueryBuilder
{
    private readonly List<string> _conditions = new List<string>();
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

    private const string BaseQuery = @"
        SELECT 
            tbl.id as tbl_id,
            tbl.code as tbl_code,
            tbl.description AS query_desc,
            tbl.ExecFunc AS exec_func,
            tbl.Query AS query, 
            tbl.exectype as exec_type,
            fld.id as fld_id, 
            fld.field_name as field_name, 
            fld.field_type as field_type, 
            fld.field_type_string as field_type_string
        FROM tblmaster tbl
        LEFT JOIN fld_query_master fld 
	        ON fld.tblmaster_id = tbl.id ";

    /// <summary>
    /// Thêm điều kiện lọc theo ExecFunc.
    /// </summary>
    public TblFldMasterQueryBuilder WhereExecFunc(string execFunc)
    {
        _conditions.Add("tbl.ExecFunc = @execFunc");
        _parameters["@execFunc"] = execFunc;
        return this;
    }

    public TblFldMasterQueryBuilder WhereCode(string code)
    {
        _conditions.Add("tbl.Code = @code");
        _parameters["@code"] = $"'{code}'";
        return this;
    }

    public TblFldMasterQueryBuilder WhereTblMasterId(string id)
    {
        _conditions.Add("tbl.id = @id");
        _parameters["@id"] = $"'{id}'";
        return this;
    }
    
    public (string Query, Dictionary<string, object> Parameters) Build()
    {
        var queryBuilder = new StringBuilder(BaseQuery);
        if (_conditions.Count > 0)
        {
            queryBuilder.Append(" WHERE ");
            queryBuilder.Append(string.Join(" AND ", _conditions));
        }
        return (queryBuilder.ToString(), _parameters);
    }
}