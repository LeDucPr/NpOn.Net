using System.Globalization;
using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using ProtoBuf;

namespace Common.Extensions.NpOn.CommonGrpcContract;

public class CommonApiResponse
{
    public CommonApiResponse()
    {
        ErrorMessages = new List<string>();
        ServerTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        ServerTimeUtc = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
    }

    [ProtoMember(1)] public bool Status { get; set; }
    [ProtoMember(2)] public EErrorCode ErrorCode { get; set; }
    [ProtoMember(3)] public List<string> ErrorMessages { get; set; }
    [ProtoMember(4)] public int Version { get; set; }
    [ProtoMember(5)] public string ServerTime { get; set; }
    [ProtoMember(7)] public string ServerTimeUtc { get; set; }
    [ProtoMember(8)] public int? TotalRow { get; set; }


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
public class CommonApiResponse<T>
{
    public CommonApiResponse()
    {
        ErrorMessages = new List<string>();
        ServerTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        ServerTimeUtc = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
    }

    [ProtoMember(1)] public bool Status { get; set; }
    [ProtoMember(2)] public EErrorCode ErrorCode { get; set; }
    [ProtoMember(3)] public List<string> ErrorMessages { get; set; }
    [ProtoMember(4)] public int Version { get; set; }
    [ProtoMember(5)] public string ServerTime { get; set; }

    [ProtoMember(6)] public T? Data { get; set; }

    //[ProtoMember(7)] public int TotalRow { get; set; }
    [ProtoMember(7)] public string ServerTimeUtc { get; set; }
    [ProtoMember(8)] public string? DataEncrypt { get; set; }
    [ProtoMember(9)] public string? KeyEncrypt { get; set; }
    [ProtoMember(10)] public int? TotalRow { get; set; }

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
}