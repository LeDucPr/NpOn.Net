using ProtoBuf;

namespace MicroServices.General.Contract.GeneralServiceContract.Queries;

[ProtoContract]
public class TblFldExecution : BaseGeneralCommand
{
    [ProtoMember(1)] public string? TblMaterId { get; set; }
    [ProtoMember(2)] public string? Code { get; set; }
    [ProtoMember(3)] public string? ExecFunc { get; set; }
    [ProtoMember(4)] public TblFldExecutionParam[]? ExecParams { get; set; }
}

[ProtoContract]
public class TblFldExecutionParam
{
    public TblFldExecutionParam()
    {
    }

    public TblFldExecutionParam(string paramName, string stringValue)
    {
        ParamName = paramName;
        StringValue = stringValue;
    }

    [ProtoMember(1)] public required string ParamName { get; set; }

    [ProtoMember(2)] public string? StringValue { get; set; }
    // [ProtoMember(3)] public Type? ParamType { get; set; } // if null => string 
}