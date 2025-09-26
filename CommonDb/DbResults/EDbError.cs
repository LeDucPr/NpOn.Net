using System.ComponentModel.DataAnnotations;

namespace CommonDb.DbResults;

public enum EDbError
{
    [Display(Name = "Unknown Error")] Unknown,


    #region Connection

    [Display(Name = "Connection Error")] Connection,

    [Display(Name = "Infrastructure Error")]
    Infrastructure,

    [Display(Name = "Session Error")] Session,

    #endregion Connection


    #region Query/Command

    [Display(Name = "Command Execution Error")]
    Command,
    
    [Display(Name = "CommandText Invalid")]
    CommandText,
    
    [Display(Name = "CommandText Syntax Invalid")]
    CommandTextSyntax,
    
    #endregion Query/Command


    #region Result

    [Display(Name = "Data Constraint Violation")]
    DataConstraint,

    [Display(Name = "Internal Application Error")]
    Internal,
    
    [Display(Name = "Get Data Error (Result is null)")]
    CannotGetData,

    [Display(Name = "Cassandra Rowset (Result is null)")]
    CassandraRowSetNull,

    [Display(Name = "Postgres DataTable (Result is null)")]
    PostgresDataTableNull,
    

    #endregion Result
}