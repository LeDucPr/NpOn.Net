using CommonDb.DbCommands;
using ProtoBuf;

namespace GeneralServiceObject.QueryObjects;

[ProtoContract]
public class TblFldExecution : BaseGeneralQuery
{
    [ProtoMember(1)] public string? TblMaterId { get; set; }
    [ProtoMember(2)] public string? Code { get; set; }
    [ProtoMember(3)] public string? ExecFunc { get; set; }
    [ProtoMember(4)] public TblFldExecutionParam[]? QueryParams { get; set; }
}

[ProtoContract]
public class TblFldExecutionParam
{
    [ProtoMember(1)] public required string ParamName { get; set; }
    [ProtoMember(2)] public string? StringValue { get; set; }
    // [ProtoMember(3)] public Type? ParamType { get; set; } // if null => string 
}