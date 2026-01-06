using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.CommonDb.DbResults.Grpc;
using ProtoBuf;

namespace Common.Extensions.NpOn.CommonGrpcContract;

[ProtoContract]
[ProtoInclude(100, typeof(CommonResponse<INpOnGrpcObject>))]
[ProtoInclude(200, typeof(CommonResponse<NpOnGrpcCell>))]
[ProtoInclude(300, typeof(CommonResponse<NpOnGrpcColumn>))]
[ProtoInclude(400, typeof(CommonResponse<NpOnGrpcRow>))]
[ProtoInclude(500, typeof(CommonResponse<NpOnGrpcTable>))]
public class CommonResponse
{
    [ProtoMember(1)] public virtual bool Status { get; set; }
    [ProtoMember(2)] public virtual EErrorCode? ErrorCode { get; set; }
    [ProtoMember(3)] public virtual List<string>? ErrorMessages { get; set; }
    [ProtoMember(4)] public virtual int Version { get; set; }
    [ProtoMember(5)] public virtual DateTime ServerTime { get; set; } = DateTime.UtcNow;
    [ProtoMember(6)] public virtual int? TotalRow { get; set; }
    
    public void SetSuccess()
    {
        Status = true;
        ErrorCode = EErrorCode.NoErrorCode;
    }

    public void SetSuccess(string message)
    {
        Status = true;
        ErrorMessages ??= [];
        ErrorMessages.Add(message);
        ErrorCode = EErrorCode.NoErrorCode;
    }

    public void SetFail(EErrorCode code)
    {
        Status = false;
        ErrorCode = code;
        string message = code.GetDisplayName();
        ErrorMessages ??= [];
        ErrorMessages.Add(message);
    }

    public void SetFail(string? message, EErrorCode code = EErrorCode.NoErrorCode)
    {
        Status = false;
        ErrorCode = code;
        ErrorMessages ??= [];
        ErrorMessages.Add(message.AsDefaultString());
    }

    public void SetFail(Exception ex, EErrorCode code = EErrorCode.NoErrorCode)
    {
        Status = false;
        ErrorCode = code;
        string message = $"Message: {ex.Message}";
        ErrorMessages ??= [];
        ErrorMessages.Add(message);
    }
    
    public void SetFail(IEnumerable<string>? messages, EErrorCode code = EErrorCode.NoErrorCode)
    {
        Status = false;
        ErrorCode = code;
        if (messages == null)
        {
            return;
        }

        foreach (var message in messages)
        {
            ErrorMessages ??= [];
            ErrorMessages.Add(message);
        }
    }

    public string? Message => ErrorMessages.AsArrayJoin();
}

[ProtoContract]
public class CommonResponse<T> : CommonResponse{
    
    [ProtoMember(1)] public override bool Status { get; set; }
    [ProtoMember(2)] public override EErrorCode? ErrorCode { get; set; }
    [ProtoMember(3)] public override List<string>? ErrorMessages { get; set; }
    [ProtoMember(4)] public override int Version { get; set; }
    [ProtoMember(5)] public override DateTime ServerTime { get; set; } = DateTime.UtcNow;
    [ProtoMember(6)] public override int? TotalRow { get; set; }
    [ProtoMember(7)] public T? Data { get; set; }
}