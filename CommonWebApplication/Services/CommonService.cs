using CommonGrpcObject;
using CommonObject.CommonObjects;
using RabbitMqBroker;

namespace CommonWebApplication.Services;

public class CommonService(ILogger<CommonService> logger) : RabbitMqEventHandler(logger)
{
    protected async Task<CommonResponse<T>> CommonProcessRbMqEvent<T>(Func<CommonResponse<T>, Task> processFunc)
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
    
    // private readonly RabbitMqConnectionPool _rabbitMqConnectionPool = contextService.RabbitMqConnectionPool;
    protected async Task<CommonResponse<T>> CommonProcess<T>(Func<CommonResponse<T>, Task> processFunc)
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
    
    protected async Task<CommonResponse> CommonProcess(Func<CommonResponse, Task> processFunc)
    {
        CommonResponse response = new CommonResponse();
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