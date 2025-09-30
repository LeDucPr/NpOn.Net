using System.ComponentModel.DataAnnotations;

namespace Enums;

public enum EDbError
{
    [Display(Name = "Unknown Error")] Unknown,


    #region Connection

    [Display(Name = "Connection Error")] Connection,

    [Display(Name = "Infrastructure Error")]
    Infrastructure,

    [Display(Name = "Session Error")] Session,
    
    [Display(Name = "Not Has Any Alive Connection")] AliveConnection,
    
    [Display(Name = "Can Not Create Connection")] CreateConnection,

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

    [Display(Name = "MongoDb BsonDocument (Result is null)")] 
    MongoDbBsonDocumentNull,

    #endregion Result
}