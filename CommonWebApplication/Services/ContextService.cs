using CommonObject;
using RabbitMqBroker;

namespace CommonWebApplication.Services;

public class ContextService(
    ILogger<ContextService> logger,
    IHttpContextAccessor? httpContextAccessor,
    IServiceProvider serviceProvider,
    RabbitMqConnectionPool rabbitMqConnectionPool,
    AuthenService authenService,
    ILogAction logAction
)
{
    public const string SessionCode = "VuaChocCho";

    // EApplicationConfiguration.RabbitMqHost.GetAppSettingConfig().AsDefaultString();
    private readonly IServiceProvider _serviceProvider =
        httpContextAccessor?.HttpContext?.RequestServices ?? serviceProvider;

    public readonly RabbitMqConnectionPool RabbitMqConnectionPool = rabbitMqConnectionPool;
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

    public AccountLoginInfo? UserInfo()
    {
        var isAuthenticated = httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true;
        if (!isAuthenticated)
        {
            return null;
        }

        string? key = SessionKeyGet();
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        var userInfo = authenService.GetLoginInfoSync(key);
        return userInfo;
    }

    public AccountLoginInfo? UserInfoBySessionId(string sessionId)
    {
        var userInfo = authenService.GetLoginInfoSync(sessionId);
        return userInfo;
    }

    public AccountLoginInfo UserInfoRequired()
    {
        var userInfo = UserInfo();
        if (userInfo == null)
        {
            throw new Exception("user info is null 3");
        }

        return userInfo;
    }

    public string? SessionKeyGet()
    {
        return httpContextAccessor?.HttpContext?.User.FindFirst(SessionCode)?.Value;
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public bool ValidateToken(string token, out ClaimsPrincipal? claimsPrincipal)
    {
        var mySecret = Encoding.UTF8.GetBytes(ConfigSettingEnum.JwtTokensKey.GetConfig());
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
            var lang = (httpContextAccessor?.HttpContext?.Request.Headers["language"]).AsEmpty();
            if (string.IsNullOrEmpty(lang))
            {
                return "vn";
            }

            return lang;
        }
    }

    public string ClientId()
    {
        var clientId = (httpContextAccessor?.HttpContext?.Request.Headers["TYTClientId"]).AsEmpty();
        return clientId;
    }

    public string DealerTMVId => ConfigSettingEnum.DealerTMVId.GetConfig();

    public string GetContextWebsiteId()
    {
        return (httpContextAccessor?.HttpContext?.Items[Constant.ContextWebsiteId]).AsEmpty();
    }

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