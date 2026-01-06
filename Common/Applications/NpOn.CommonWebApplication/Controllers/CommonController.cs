using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace Common.Extensions.NpOn.CommonWebApplication.Controllers;

// [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
// [EnableCors(Constant.CorsPolicy)]
// [Produces("application/json")]
// [Route("api/[controller]/[action]")]
public class CommonController(
    ILogger<CommonController> logger,
    ContextService contextService) : ControllerBase
{
    protected async Task<CommonApiResponse<T>> ProcessRequest<T>(Func<CommonApiResponse<T>, Task> processFunc)
    {
        return await ProcessRequest(null, processFunc);
    }

    protected async Task<CommonApiResponse<T>> ProcessRequest<T>(object? request,
        Func<CommonApiResponse<T>, Task> processFunc)
    {
        CommonApiResponse<T> response = new CommonApiResponse<T>();
        try
        {
            await processFunc(response);
        }
        catch (Exception e)
        {
            HandlerException(e, response);
        }

        return response;
    }

    protected async Task<CommonApiResponse> ProcessRequest(Func<CommonApiResponse, Task> processFunc)
    {
        CommonApiResponse response = new CommonApiResponse();
        try
        {
            await processFunc(response);
        }
        catch (Exception e)
        {
            HandlerException(e, response);
        }

        return response;
    }


    protected void HandlerException<T>(Exception e, CommonApiResponse<T> response)
    {
        if (e.GetType() == typeof(RpcException) &&
            ((RpcException)e).StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            response.SetFail(EErrorCode.Unauthorized);
            return;
        }

        LogError(e);
        if (EApplicationConfiguration.IsDevEnvironment.GetAppSettingConfig().AsDefaultBool())
        {
            throw e;
        }

        response.SetFail(EErrorCode.InternalExceptions);
    }

    protected void HandlerException(Exception e, CommonApiResponse response)
    {
        if (e.GetType() == typeof(RpcException) &&
            ((RpcException)e).StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            response.SetFail(EErrorCode.Unauthorized);
            return;
        }

        LogError(e);
        if (EApplicationConfiguration.IsDevEnvironment.GetAppSettingConfig().AsDefaultBool())
        {
            throw e;
        }

        response.SetFail(EErrorCode.InternalExceptions);
    }

    protected IActionResult HandlerException(Exception e)
    {
        LogError(e);
        if (e.GetType() == typeof(RpcException) &&
            ((RpcException)e).StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            return Redirect(EApplicationConfiguration.UnauthenticatedAccountUrl.GetAppSettingConfig()
                .AsDefaultString());
        }

        LogError(e);
        if (EApplicationConfiguration.IsDevEnvironment.GetAppSettingConfig().AsDefaultBool())
        {
            throw e;
        }

        return Redirect(EApplicationConfiguration.ExceptionUrl.GetAppSettingConfig().AsDefaultString());
    }


    #region Log

    protected void LogError(Exception exception, string message)
    {
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   { "RefererUrl", contextService.RefererUrl() }
               }))
        {
            logger.LogError(exception, "CommonController Exception {Message}", message);
        }
    }

    protected void LogError(Exception exception, string message, params object?[] args)
    {
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   { "RefererUrl", contextService.RefererUrl() }
               }))
        {
            logger.LogError(exception, "CommonController Exception {Message}", [..(object[])[message], ..args]);
        }
    }

    protected void LogError(Exception exception, params object?[] args)
    {
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   { "RefererUrl", contextService.RefererUrl() }
               }))
        {
            logger.LogError(exception, "CommonController Exception {Message}", args);
        }
    }

    protected void LogError(Exception exception)
    {
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   { "RefererUrl", contextService.RefererUrl() }
               }))
        {
            logger.LogError(exception, "CommonController Exception {Message}", exception.Message);
        }
    }

    protected void LogError(IEnumerable<string>? message)
    {
        if (message == null)
        {
            return;
        }

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   { "RefererUrl", contextService.RefererUrl() }
               }))
        {
            logger.LogError("CommonController Error {Message}", message);
        }
    }

    protected void LogError(string? message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   { "RefererUrl", contextService.RefererUrl() }
               }))
        {
            logger.LogError("CommonController Error {Message}", message);
        }
    }

    protected void LogWarning(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   { "RefererUrl", contextService.RefererUrl() }
               }))
        {
            logger.LogError("CommonController Warning {Message}", message);
        }
    }

    protected void LogWarning(string message, params object?[] args)
    {
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   { "RefererUrl", contextService.RefererUrl() }
               }))
        {
            logger.LogWarning("CommonController Warning {Message}", [..(object[])[message], ..args]);
        }
    }

    protected void LogInformation(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   { "RefererUrl", contextService.RefererUrl() }
               }))
        {
            logger.LogInformation("CommonController Information {Message}", message);
        }
    }

    #endregion Log
}