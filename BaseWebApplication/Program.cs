using BaseWebApplication.Builders;
using BaseWebApplication.Parameters;
using BaseWebApplication.Services;
using CommonMode;
using CommonObject;
using Enums;

namespace BaseWebApplication;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = BuilderExtensions.CreateDefaultBuilder(args).GetAwaiter().GetResult();

        // Sử dụng async để lambda trả về Task, đúng với yêu cầu của AddCollectionServices
        // Giờ đây bạn có thể đăng ký các dịch vụ bên trong
        await builder.Services.AddCollectionServices(async (services) =>
        {
            services.AddGrpc();
            return services;
        });

        var app = builder.Build();

        //// Configure the HTTP request pipeline.
        app.MapGrpcService<GreeterService>();
        app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }
}