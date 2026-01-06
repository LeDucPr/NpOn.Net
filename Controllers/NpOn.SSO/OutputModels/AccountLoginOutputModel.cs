using Definitions.NpOn.ProjectEnums.AccountEnums;

namespace Controllers.NpOn.SSO.OutputModels;

public class AccountLoginOutputModel
{
    public required Guid AccountId { get; set; }
    public required EAuthentication AuthType { get; set; }
    public required ELoginType LoginType { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? CreatedAt { get; set; }
    public required string SessionId { get; set; }
    public int MinuteExpire { get; set; }
}