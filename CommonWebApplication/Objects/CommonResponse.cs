using CommonMode;
using CommonObject;
using Enums;
using ProtoBuf;

namespace CommonWebApplication.Objects;

[ProtoContract]
public class CommonResponse
{
    [ProtoMember(1)] public bool Status { get; set; }
    [ProtoMember(2)] public EErrorCodeEnum? ErrorCode { get; set; }
    [ProtoMember(3)] public List<string>? ErrorMessages { get; set; }
    [ProtoMember(4)] public int Version { get; set; }
    [ProtoMember(5)] public DateTime ServerTime { get; set; } = DateTime.UtcNow;
    [ProtoMember(6)] public int? TotalRow { get; set; }
    
    
    public void SetSuccess()
    {
        Status = true;
        ErrorCode = EErrorCodeEnum.NoErrorCode;
    }

    public void SetSuccess(string message)
    {
        Status = true;
        ErrorMessages ??= [];
        ErrorMessages.Add(message);
        ErrorCode = EErrorCodeEnum.NoErrorCode;
    }

    public void SetFail(EErrorCodeEnum code)
    {
        Status = false;
        ErrorCode = code;
        string message = code.GetDisplayName();
        ErrorMessages ??= [];
        ErrorMessages.Add(message);
    }

    public void SetFail(string? message, EErrorCodeEnum code = EErrorCodeEnum.NoErrorCode)
    {
        Status = false;
        ErrorCode = code;
        ErrorMessages ??= [];
        ErrorMessages.Add(message.AsDefaultString());
    }

    public void SetFail(Exception ex, EErrorCodeEnum code = EErrorCodeEnum.NoErrorCode)
    {
        Status = false;
        ErrorCode = code;
        string message = $"Message: {ex.Message}";
        ErrorMessages ??= [];
        ErrorMessages.Add(message);
    }
    
    public void SetFail(IEnumerable<string>? messages, EErrorCodeEnum code = EErrorCodeEnum.NoErrorCode)
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