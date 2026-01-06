using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonGrpcContract;

namespace Common.Extensions.NpOn.CommonWebApplication.Services;

public class CommonService(ILogger<CommonService> logger) //: RabbitMqEventHandler(logger)
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
            response.SetFail($"An unexpected error occurred: {e.Message}");
            logger.LogError(e, "An error occurred in CommonProcessRbMqEvent: {ErrorMessage}", e.Message);
        }

        return response;
    }

    // private readonly RabbitMqConnectionPool _rabbitMqConnectionPool = contextService.RabbitMqConnectionPool;

    protected async Task<CommonResponse<T>> CommonProcess<T>(
        params Func<CommonResponse<T>, Task<(CommonResponse<T> response, EControlFlow flow)>>[] processFunctions)
    {
        CommonResponse<T> response = new CommonResponse<T>();
        try
        {
            foreach (var processFunc in processFunctions)
            {
                var (resp, flow) = await processFunc(response);
                response = resp;
                if (!resp.Status)
                    break;
                if (flow == EControlFlow.Continue)
                    continue;
                if (flow == EControlFlow.Break)
                    break;
            }
        }
        catch (Exception e)
        {
            response.SetFail($"An unexpected error occurred: {e.Message}");
            logger.LogError(e, "An error occurred in CommonProcess: {ErrorMessage}", e.Message);
        }

        return response;
    }

    protected async Task<CommonResponse<T>> CommonProcess<T>(Func<CommonResponse<T>, Task> processFunc)
    {
        CommonResponse<T> response = new CommonResponse<T>();
        try
        {
            await processFunc(response);
        }
        catch (Exception e)
        {
            response.SetFail($"An unexpected error occurred: {e.Message}");
            logger.LogError(e, "An error occurred in CommonProcess: {ErrorMessage}", e.Message);
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
            response.SetFail($"An unexpected error occurred: {e.Message}");
            logger.LogError(e, "An error occurred in CommonProcess: {ErrorMessage}", e.Message);
        }

        return response;
    }
}