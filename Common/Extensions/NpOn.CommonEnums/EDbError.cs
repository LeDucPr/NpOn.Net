using System.ComponentModel.DataAnnotations;

namespace Common.Extensions.NpOn.CommonEnums;

public enum EDbError
{
    [Display(Name = "Unknown Error")] Unknown,

    [Display(Name = "Connect option not found")]
    ConnectOption,
    [Display(Name = "Field not found")] FieldNotFound,

    [Display(Name = "Join type not supported")]
    JoinTypeNotSupported,

    #region Connection

    [Display(Name = "Connection Error")] Connection,

    [Display(Name = "Infrastructure Error")]
    Infrastructure,

    [Display(Name = "Session Error")] Session,

    [Display(Name = "Not Has Any Alive Connection")]
    AliveConnection,

    [Display(Name = "Can Not Create Connection")]
    CreateConnection,

    #endregion Connection


    #region Query/Command

    [Display(Name = "Command Execution Error")]
    Command,

    [Display(Name = "Command Parameter Type Error ")]
    CommandParam,

    [Display(Name = "Execution Function Error")]
    ExecFuncName,

    [Display(Name = "CommandText Invalid")]
    CommandText,

    [Display(Name = "CommandText Syntax Invalid")]
    CommandTextSyntax,

    [Display(Name = "Redis Command Invalid")]
    CommandNotSupported,

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

    [Display(Name = "Mssql DataTable (Result is null)")]
    MssqlDataTableNull,

    [Display(Name = "MongoDb BsonDocument (Result is null)")]
    MongoDbBsonDocumentNull,

    [Display(Name = "Redis Value (Result is null)")]
    RedisValueIsNull,
    
    [Display(Name = "Redis Execute Error")]
    RedisExecute,

    #endregion Result
}