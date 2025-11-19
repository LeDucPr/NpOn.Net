using CommonMode;
using CommonObject;
using CommonWebApplication;
using DbFactory;
using Enums;
using IQuestionService;
using QuestionService.Services;

namespace QuestionService;

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

        // Add Service
        services.AddTransient<IFaqService, FaqService>();
        services.AddTransient<ISurveyService, SurveyService>();

        return Task.CompletedTask;
    }

    protected override void ConfigureBasePipeline(WebApplication app)
    {
        app.MapGet("/", () => "QuestionService");
        base.ConfigureBasePipeline(app);
    }

    protected override Task ConfigurePipeline(WebApplication app)
    {
        // Add Map Grpc Service
        app.MapGrpcService<FaqService>();
        app.MapGrpcService<SurveyService>();
        return Task.CompletedTask;
    }
}