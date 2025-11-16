namespace SSO.Requests;

public class AccountLoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}