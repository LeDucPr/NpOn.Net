using ProjectEnums.AccountEnums;

namespace SSO.Requests;

public class AccountLoginRequest
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? UserName { get; set; }
    public required string Password { get; set; }
    public string? DeviceInfo { get; set; }
    public ELoginType? LoginType { get; set; }
    public required EAuthentication AuthType { get; set; }
    public string? AppId { get; set; }
}

public class AccountRefreshTokenRequest
{
    public required string RefreshToken { get; set; }
    public string? DeviceInfo { get; set; }
    public ELoginType? LoginType { get; set; }
    public required EAuthentication AuthType {get; set; }
    public string? ReturnUrl { get; set; }
}