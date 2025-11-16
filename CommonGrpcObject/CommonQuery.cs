using Enums;
using ProtoBuf;

namespace CommonGrpcObject;

[ProtoContract]
[ProtoInclude(1000, typeof(CommonQuery))]
public abstract class GrpcQuery
{
    [ProtoMember(1)] public virtual DateTime QueryUtcTime { get; init; }
}

[ProtoContract]
[ProtoInclude(2000, typeof(CommonJsonQuery))]
public class CommonQuery : GrpcQuery
{
    [ProtoMember(1)] public virtual bool Status { get; set; }
    [ProtoMember(2)] public virtual EErrorCode? ErrorCode { get; set; }
    [ProtoMember(3)] public virtual string? Object { get; set; }
    [ProtoMember(4)] public override DateTime QueryUtcTime { get; init; } // = DateTime.UtcNow;
}

[ProtoContract]
public class CommonJsonQuery : CommonQuery
{
    [ProtoMember(1)] public required string Json { get; set; }
}

#region Abstract Class

public abstract class CommonAbsQuery : GrpcQuery
{
    public abstract bool Status { get; set; }
    public abstract EErrorCode? ErrorCode { get; set; }
    public abstract string? Object { get; set; }
    public abstract override DateTime QueryUtcTime { get; init; } // = DateTime.UtcNow;
}

#endregion Abstract Class