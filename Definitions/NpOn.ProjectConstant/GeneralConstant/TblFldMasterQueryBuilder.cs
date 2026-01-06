using System.Text;

namespace Definitions.NpOn.ProjectConstant.GeneralConstant;

public class TblFldMasterQueryBuilder
{
    private readonly List<string> _conditions = new List<string>();
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

    private const string BaseQuery = @"
        SELECT
            vw.tbl_id       AS tbl_id,
            vw.tbl_code     AS tbl_code,
            vw.query_desc   AS query_desc,
            vw.exec_func    AS exec_func,
            vw.query        AS query,
            vw.exec_type    AS exec_type,
            vw.service_name AS service_name,
            vw.db_type      AS db_type,
            vw.fld_id       AS fld_id,
            vw.order_sort   AS order_sort,
            vw.field_name   AS field_name,
            vw.field_type   AS field_type,
            vw.field_type_string AS field_type_string
        FROM vw_query_master vw ";

    // SELECT 
    //     tbl.id AS tbl_id,
    //     tbl.code AS tbl_code,
    //     tbl.description AS query_desc,
    //     tbl.ExecFunc AS exec_func,
    //     tbl.Query AS query, 
    //     tbl.exectype AS exec_type,
    //     tbl.service_name AS service_name,
    //     tbl.db_type AS db_type,
    //     fld.id AS fld_id, 
    //     fld.order_sort AS order_sort,
    //     fld.field_name AS field_name, 
    //     fld.field_type AS field_type, 
    //     fld.field_type_string AS field_type_string
    // FROM tblmaster tbl
    // LEFT JOIN fld_query_master fld 
    //     ON fld.tblmaster_id = tbl.id


    public TblFldMasterQueryBuilder WhereExecFunc(string execFunc)
    {
        _conditions.Add("vw.exec_func = @execFunc");
        _parameters["@execFunc"] = execFunc;
        return this;
    }

    public TblFldMasterQueryBuilder WhereCode(string code)
    {
        _conditions.Add("vw.tbl_code = @code");
        _parameters["@code"] = code;
        return this;
    }

    public TblFldMasterQueryBuilder WhereTblMasterId(string id)
    {
        _conditions.Add("vw.tbl_id = @id");
        _parameters["@id"] = id;
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

        queryBuilder.Append(" ORDER BY vw.order_sort");

        return (queryBuilder.ToString(), _parameters);
    }
}