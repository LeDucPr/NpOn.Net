using System.Collections.Concurrent;
using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonObject.CommonObjects;
using CommonWebApplication.Services;
using DbFactory;
using IFaCareTestService;

namespace FaCareTestService.Services;

public class TestQueueService(
    IDbFactoryWrapper dbFactoryWrapper,
    ILogger<CommonService> logger) : CommonService(logger), ITestQueueService
{
    public async Task<CommonResponse<string>> TestRabbitMqHandler()
    {
        return await CommonProcess<string>(async (response) =>
        {
            
        });
    }
    
    public async Task<CommonResponse<INpOnGrpcObject>> TestQueue2C()
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            await T2Queue();
            response.SetSuccess("Queue processing completed.");
        });
    }

    private async Task<CommonResponse<INpOnGrpcObject>> T2Queue()
    {
        var response = new CommonResponse<INpOnGrpcObject>();
        try
        {
            int maxEntranceCount = 20;
            var entranceQueue = new ConcurrentQueue<INpOnWrapperResult>();
            var consumptionQueue = new ConcurrentQueue<INpOnWrapperResult>();

            // tạo 2 task chạy song song với nhau sao cho dữ liệu trong entranceQueue không bao giờ vượt quá max, nếu vượt quá thì chờ trong vòng lặp 
            var producerTask = Task.Run(async () =>
            {
                const int chunkSize = 500;
                long mlsTime = 0;
                int countLoop = 0;
                string? lastGuidStr = null;

                while (true)
                {
                    while (entranceQueue.Count >= maxEntranceCount)
                    {
                        await Task.Delay(20);
                    }

                    string pgQuery;
                    if (lastGuidStr == null)
                        pgQuery = $"SELECT * FROM data_t_duc_test_5mil ORDER BY _id DESC LIMIT {chunkSize}";
                    else
                        // next  batch get by  last_guid (string)
                        pgQuery =
                            $"SELECT * FROM data_t_duc_test_5mil WHERE _id < '{lastGuidStr}' ORDER BY _id DESC LIMIT {chunkSize}";

                    INpOnWrapperResult? mockResult = await dbFactoryWrapper.QueryAsync(pgQuery);
                    if (mockResult is INpOnTableWrapper { RowWrappers.Count: > 0 } tableWrapper)
                    {
                        var ids = tableWrapper.CollectionWrappers
                            .GetColumnWrapperByColumnNames(["_id"])
                            .FirstOrDefault().Value?.GetColumnWrapper();
                        lastGuidStr = ids?.Last().Value.ValueAsObject?.ToString();
                    }

                    mlsTime += mockResult?.QueryTimeMilliseconds ?? 0;

                    if (mockResult != null)
                        entranceQueue.Enqueue(mockResult);

                    Console.WriteLine(
                        $"CountLoop: {++countLoop}   --   Guid: {lastGuidStr}   -----   ElapsedTime: {mlsTime} ms   ----   QueryTime: {mockResult?.QueryTimeMilliseconds} ");
                }
            });

            var consumerTask = Task.Run(function: async () =>
            {
                int batchCount = 0;
                while (true)
                {
                    if (consumptionQueue.Count == 0)
                    {
                        bool isHasValue = entranceQueue.TryDequeue(out var item);
                        if (isHasValue && item != null)
                            consumptionQueue.Enqueue(item);
                    }
                    else
                        await Task.Delay(100);

                    bool isHasConsumption = entranceQueue.TryDequeue(out var consumptionResult);
                    if (!isHasConsumption)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    if (consumptionResult is INpOnTableWrapper { RowWrappers.Count: > 0 } tableWrapper)
                    {
                        var ids = tableWrapper.CollectionWrappers
                            .GetColumnWrapperByColumnNames(["_id"])
                            .FirstOrDefault().Value?.GetColumnWrapper();

                        if (ids is { Count: > 0 })
                        {
                            Console.WriteLine(
                                $"\u001b[32m   ----   Batch: {++batchCount} with last index: {ids.Last().Value.ValueAsObject}\u001b[0m");
                            await Task.Delay(200);
                        }
                    }
                }
            });

            await Task.WhenAll(producerTask, consumerTask);
            response.SetSuccess("Queue processing completed.");
        }
        catch (Exception e)
        {
            response.SetFail(e);
            logger.LogError(e, "Error in TestCallSgnR");
        }

        return response;
    }
}