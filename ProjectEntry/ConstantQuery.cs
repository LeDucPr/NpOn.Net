namespace ProjectEntry;

public class ConstantQuery
{
    public const string TblFldMaster = @"
        SELECT 
            tbl.description AS query_desc,
            tbl.ExecFunc,
            tbl.Query, 
            fld.field_name, 
            fld.field_type
        FROM 
            fld_query_master fld
        JOIN 
            tblmaster tbl ON fld.tblmaster_id = tbl.id
        WHERE 
            tbl.ExecFunc = @execFunc;
        ";
}