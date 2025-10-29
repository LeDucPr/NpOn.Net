using CommonWebApplication.Builders;

namespace CommonWebApplication;

public abstract class CommonProgram
{
    protected readonly WebApplicationBuilder Builder;
    protected CommonProgram(string[] args)
    {
        Builder = BuilderExtensions.CreateDefaultBuilder(args).GetAwaiter().GetResult();
        Base().GetAwaiter().GetResult();
    }

    private async Task Base()
    {
        await Builder.Services.AddCollectionServices(async (services) =>
        {
            services.AddGrpc();
            return services;
        });

        await Init();
        
        var app = Builder.Build();

        // app.MapGrpcService<GreeterService>();
        app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }

    /// <summary>
    /// Builder.Services.AddCollectionServices --> Add Service use
    /// </summary>
    /// <returns></returns>
    protected abstract Task Init();
}