namespace ProjectEntry.AccountEntries;

public static class AuthenServiceQueryCode
{
    // account
    public const string AccountGetByUsernameAndPassword = "account_get_by_username_and_password";
    // account info
    public const string AccountLoginInfoGetByUsernameAndPassword = "account_login_info_get_by_refresh_token";
    public const string AccountLoginInfoGetByAccountId = "account_login_info_get_by_account_id";
    public const string AccountLoginInfoGetBySessionId = "account_login_info_get_by_session_id";
    public const string AccountLoginInfoSaveLogin = "account_login_info_save";
}