using CommonMode;
using CommonObject;
using CommonWebApplication;
using DbFactory;
using Enums;
using FaCareTestService.Services;
using IFaCareTestService;
using FaCareTestService.SignalR;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace FaCareTestService;

public sealed class Program : CommonProgram
{
    private Program(string[] args) : base(args)
    {
    }

    public static async Task Main(string[] args)
    {
        Program program = new Program(args);
        await program.RunAsync();
    }

    protected override Task ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDbFactoryWrapper>(factory =>
        {
            string connectionString =
                EApplicationConfiguration.ConnectionString.GetAppSettingConfig().AsDefaultString();
            int connectionNumber = EApplicationConfiguration.ConnectionNumber.GetAppSettingConfig().AsDefaultInt();
            IDbFactoryWrapper factoryWrapper =
                new DbFactoryWrapper(connectionString, EDb.Postgres, connectionNumber, true);
            // string stringQuery = "select * from Users where id = 'C000175'";
            // INpOnWrapperResult? resultOfQuery = factoryWrapper?.QueryAsync(stringQuery).GetAwaiter().GetResult();
            return factoryWrapper;
        });

        if (EApplicationConfiguration.IsStartAsync.GetAppSettingConfig().AsDefaultBool())
        {
            services.AddHostedService<HostingApp>();
        }

        services.AddSignalR(); // Add SignalR

        // Add Service
        services.AddTransient<IFaCareTService, FaCareTService>();
        services.AddSingleton<ITestQueueService, TestQueueService>();
        return Task.CompletedTask;
    }

    protected override void ConfigureBasePipeline(WebApplication app)
    {
        app.MapGet("/", () => "FaCareTestService");
        base.ConfigureBasePipeline(app); // validate 
    }

    protected override Task ConfigurePipeline(WebApplication app)
    {
        // Add Map Grpc Service
        app.MapHub<FaCareHub>("/FaCareTSgnRPushing");

        // TẠO "CẦU NỐI" HTTP ĐỂ TRÌNH DUYỆT CÓ THỂ GỌI
        // Endpoint: GET /trigger-stream?connectionId=...
        app.MapGet("/trigger-stream", async (
            [FromQuery] string connectionId,
            [FromServices] IFaCareTService faCareService) =>
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                return Results.BadRequest("Missing 'connectionId' query parameter.");
            }

            // Create metadata and CallContext to call virtual SignalR
            var headers = new Metadata { { "X-Connection-Id", connectionId } };
            var callOptions = new CallOptions(headers);

            // call gRPC with internal server with created context 
            var response = await faCareService.TestCallSgnR(callOptions);
            return Results.Ok(response);
        });

        app.MapGrpcService<FaCareTService>();
        return Task.CompletedTask;
    }
}