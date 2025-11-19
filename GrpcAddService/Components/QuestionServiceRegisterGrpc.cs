using CommonMode;
using CommonObject;
using Enums;
using IAccountService;
using IQuestionService;

namespace GrpcAddService.Components;

public static partial class ServiceRegisterGrpc
{
    public static IServiceCollection QuestionServiceRegisterGrpc(this IServiceCollection services)
    {
        var questionServiceUrl =
            EApplicationConfiguration.QuestionServiceUrl.GetAppSettingConfig().AsDefaultString();
        if (string.IsNullOrWhiteSpace(questionServiceUrl))
            return services;
        services.RegisterGrpcClientLoadBalancing<IFaqService>(questionServiceUrl);
        services.RegisterGrpcClientLoadBalancing<ISurveyService>(questionServiceUrl);
        return services;
    }
}