using CommonWebApplication.Objects;

namespace CommonWebApplication.Services;

public class CommonService(ILogger<CommonService> logger)
{
    // private readonly RabbitMqConnectionPool _rabbitMqConnectionPool = contextService.RabbitMqConnectionPool;
    protected async Task<CommonResponse<T>> ProcessCommand<T>(Func<CommonResponse<T>, Task> processFunc)
    {
        CommonResponse<T> response = new CommonResponse<T>();
        try
        {
            await processFunc(response);
        }
        catch (Exception e)
        {
            response.SetFail(e.Message);
            logger.LogError($"{e.Message}");
        }
        return response;
    }
}