namespace Common.Extensions.NpOn.CommonEnums;

public enum EErrorCode
{
    NoErrorCode = 0,
    Success = 1,
    Fail = 2,
    ErrorCommentLimit = 3,
    ErrorCommentTime = 4,
    
    // HTTP Status Code Style
    NotFound = 404,
    UserNotFound = 404, // Can be the same as NotFound or a more specific one
    Unauthorized = 401,
    PermissionDeny = 403,
    
    // Internal Errors
    InternalExceptions = 500,
    NullRequestExceptions = 501,
    AntiXss = 502,
    NotExistExceptions = 503,
    UserNullException = 504,
    IdNullException = 505,
    CurrentWebsiteNullException = 506,
    CurrentCompanyNullException = 507,
    InternalExceptionsNotDefine = 508,
    InternalExceptionsInService = 509,
    AccountPasswordIsConfigured = 510,
    OtpSendLimit = 511,
    OtpVerifyLimit = 512,
    OtpInvalid = 513,
    DataProcessingError = 530,

    // gRPC Specific Errors
    Grpc = 514,
    GrpcUnimplemented = 515,
    GrpcUnavailable = 516,
    GrpcInternal = 517,
    GrpcCancelled = 518,
    GrpcDeadlineExceeded = 519,
    GrpcNotFound = 520,
    GrpcAlreadyExists = 521,
    GrpcPermissionDenied = 522,
    GrpcResourceExhausted = 523,
    GrpcFailedPrecondition = 524,
    GrpcAborted = 525,
    GrpcOutOfRange = 526,
    GrpcUnauthenticated = 527,
    GrpcDataLoss = 528,
    GrpcUnknown = 529
}
