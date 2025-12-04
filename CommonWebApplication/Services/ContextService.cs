using System.IdentityModel.Tokens.Jwt;
using System.Net;
// using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using AccountServiceObject.BusinessObjects;
using CommonMode;
using CommonObject;
using Enums;
using Microsoft.IdentityModel.Tokens;
using ProjectEnums.AccountEnums;
using RabbitMqBroker;

namespace CommonWebApplication.Services;

public class ContextService(
    ILogger<ContextService> logger,
    IHttpContextAccessor? httpContextAccessor,
    IServiceProvider serviceProvider,
    // RabbitMqConnectionPool rabbitMqConnectionPool,
    AuthenService authenService,
    ILogAction logAction
)
{
    // for header 
    public const string HeaderLanguage = "language";
    public const string SessionCode = "VuaChocCho";
    public const string LoginTypeEnumCode = "LoginType";
    public const string TokenCreatedUtc = "TokenCreatedUtc";
    public const string Permission = "PermissionSession";
    public const string SessionIdPrefix = "SESSIONID";
    public const string MinuteExpirePrefix = "MinuteExpire";

    private const string DefaultLang = "vi";

    // EApplicationConfiguration.RabbitMqHost.GetAppSettingConfig().AsDefaultString();
    private readonly IServiceProvider _serviceProvider =
        httpContextAccessor?.HttpContext?.RequestServices ?? serviceProvider;

    // public readonly RabbitMqConnectionPool RabbitMqConnectionPool = rabbitMqConnectionPool;
    public readonly ILogAction LogAction = logAction;

    public string GetIp()
    {
        var result = string.Empty;
        try
        {
            //first try to get IP address from the forwarded header
            if (httpContextAccessor?.HttpContext?.Request.Headers != null)
            {
                //the X-Forwarded-For (XFF) HTTP header field is a de facto standard for identifying the originating IP address of a client
                //connecting to a web server through an HTTP proxy or load balancer
                var forwardedHttpHeaderKey = "X-FORWARDED-FOR";
                var forwardedHeader = httpContextAccessor.HttpContext.Request.Headers[forwardedHttpHeaderKey];
                if (!string.IsNullOrEmpty(forwardedHeader))
                    result = forwardedHeader.FirstOrDefault();
            }

            // if this header not exists try get connection remote IP address
            if (string.IsNullOrEmpty(result) &&
                httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress != null)
                result = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }
        catch
        {
            return string.Empty;
        }

        if (result != null && result.Equals("::1", StringComparison.InvariantCultureIgnoreCase))
            result = "127.0.0.1"; // if null using localhost 
        return result.AsDefaultString();
    }

    #region user info

    public string? GetAccountIdAsString()
    {
        var user = httpContextAccessor?.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return null;
        // private AuthenticationService.CreateToken
        return user.FindFirst(JwtRegisteredClaimNames.Sid)?.Value.AsDefaultString();
    }

    public AccountLoginInfoObject? UserInfo()
    {
        var isAuthenticated = httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true;
        if (!isAuthenticated)
            return null;

        string? key = GetSessionKey();
        if (string.IsNullOrEmpty(key))
            return null;

        var userInfo = authenService.GetLoginInfoSync(key);
        return userInfo;
    }

    public AccountLoginInfoObject UserInfoRequired()
    {
        var userInfo = UserInfo();
        if (userInfo == null)
        {
            throw new Exception("user info is null 3");
        }

        return userInfo;
    }

    public AccountLoginInfoObject? UserInfoBySessionId(string sessionId)
    {
        var userInfo = authenService.GetLoginInfoSync(sessionId);
        return userInfo;
    }

    #endregion user info

    public string? GetSessionKey()
    {
        return httpContextAccessor?.HttpContext?.User.FindFirst(SessionCode)?.Value;
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public bool ValidateToken(string token, out ClaimsPrincipal? claimsPrincipal)
    {
        var mySecret =
            Encoding.UTF8.GetBytes(EApplicationConfiguration.JwtTokensKey.GetAppSettingConfig().AsDefaultString());
        var mySecurityKey = new SymmetricSecurityKey(mySecret);
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = mySecurityKey,
                ValidateIssuer = false,
                ValidateAudience = false,
            }, out SecurityToken validatedToken);
        }
        catch (Exception exception)
        {
            claimsPrincipal = null;
            logger.LogWarning(exception, "{Message}", exception.Message);
            return false;
        }

        return true;
    }

    public string RefererUrl()
    {
        if (httpContextAccessor?.HttpContext?.Request is null)
        {
            return string.Empty;
        }

        return httpContextAccessor.HttpContext.Request.GetTypedHeaders().Referer?.OriginalString ?? string.Empty;
    }

    public string LanguageId
    {
        get
        {
            var lang = (httpContextAccessor?.HttpContext?.Request.Headers[HeaderLanguage]).AsDefaultString();
            if (string.IsNullOrEmpty(lang))
            {
                return DefaultLang;
            }

            return lang;
        }
    }

    private string CheckAndReturnHeaderFromSession() // clientId
    {
        var clientId = (httpContextAccessor?.HttpContext?.Request.Headers[SessionCode]).AsDefaultString();
        return clientId; // maybe null => empty string
    }

    public string ClientId => CheckAndReturnHeaderFromSession();

    public void Set404()
    {
        httpContextAccessor!.HttpContext!.Response.StatusCode = (int)HttpStatusCode.NotFound;
    }

    public HttpRequest Request()
    {
        return httpContextAccessor!.HttpContext!.Request;
    }

    public HttpResponse Response()
    {
        return httpContextAccessor!.HttpContext!.Response;
    }
}