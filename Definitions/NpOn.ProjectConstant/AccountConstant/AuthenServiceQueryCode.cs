namespace Definitions.NpOn.ProjectConstant.AccountConstant;

public static class AuthenServiceQueryCode
{
    // account
    public const string AccountGetByUsernameAndPassword = "account_get_by_username_and_password";
    public const string AccountGetByUsernameOrPhoneNumberOrEmail = "account_get_by_username_or_phone_number_or_email";
    // account info
    public const string AccountLoginInfoGetByRefreshToken = "account_login_info_get_by_refresh_token";
    // public const string AccountLoginInfoGetByAccountId = "account_login_info_get_by_account_id";
    public const string AccountLoginInfoGetBySessionId = "account_login_info_get_by_session_id";
    public const string AccountGetById = "account_get_by_id";
    public const string AccountGetByIds = "account_get_by_ids";
}