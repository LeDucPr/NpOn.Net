using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonObject.CommonObjects;
using DbFactory;
using FaCareTestService.SignalR;
using IFaCareTestService;
using Microsoft.AspNetCore.SignalR;
using ProtoBuf.Grpc;
using System.Collections.Concurrent;

namespace FaCareTestService.Services;

public class FaCareTService(
    IDbFactoryWrapper dbFactoryWrapper,
    ILogger<FaCareTService> logger,
    IHubContext<FaCareHub> hubContext // Inject IHubContext<T>
) : IFaCareTService
{
    // Phương thức này cần được gọi từ một gRPC call, không phải từ SignalR client
    public async Task<CommonResponse<INpOnGrpcObject>> TestCallSgnR(CallContext context = default)
    {
        var response = new CommonResponse<INpOnGrpcObject>();
        try
        {
            var connectionId = context.RequestHeaders?.GetValue("X-Connection-Id");
            // if (string.IsNullOrEmpty(connectionId))
            // {
            //     response.SetFail("Missing 'X-Connection-Id' in gRPC metadata.");
            //     return response;
            // }


            _ = Task.Run(async () =>
            {
                const int chunkSize = 5000;
                string? lastGuidStr = null;
                bool hasMoreData = true;

                long mlsTime = 0;
                int countLoop = 0;

                while (hasMoreData)
                {
                    string pgQuery;
                    if (lastGuidStr == null)
                    {
                        // batch đầu tiên
                        pgQuery = $"SELECT _id FROM data_t_ ORDER BY _id DESC LIMIT {chunkSize}";
                    }
                    else
                    {
                        // các batch tiếp theo dựa vào last_guid string
                        pgQuery =
                            $"SELECT _id FROM data_t_ WHERE _id < '{lastGuidStr}' ORDER BY _id DESC LIMIT {chunkSize}";
                    }

                    INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.QueryAsync(pgQuery);

                    if (resultOfQuery is INpOnTableWrapper { RowWrappers.Count: > 0 } tableWrapper)
                    {
                        var ids = tableWrapper.CollectionWrappers
                            .GetColumnWrapperByColumnNames(["_id"])
                            .FirstOrDefault().Value?.GetColumnWrapper();

                        if (ids is { Count: > 0 })
                        {
                            // gửi batch về client
                            // await hubContext.Clients.Client(connectionId)
                            //     .SendAsync("ReceiveIdStream", ids, context.CancellationToken);

                            // cập nhật lastGuidStr bằng giá trị cuối cùng trong batch
                            lastGuidStr = ids.Last().Value?.ValueAsObject?.ToString();
                        }
                    }
                    else
                    {
                        hasMoreData = false;
                    }

                    mlsTime += resultOfQuery?.QueryTimeMilliseconds ?? 0;
                    Console.WriteLine(
                        $"CountLoop: {++countLoop}   --   Guid: {lastGuidStr}   -----   ElapsedTime: {mlsTime} ms   ----   QueryTime: {resultOfQuery?.QueryTimeMilliseconds} ");
                }
            });

            response.SetSuccess("Streaming process started.");
        }
        catch (Exception e)
        {
            response.SetFail(e);
            logger.LogError(e, "Error in TestCallSgnR");
        }

        return response;
    }

}