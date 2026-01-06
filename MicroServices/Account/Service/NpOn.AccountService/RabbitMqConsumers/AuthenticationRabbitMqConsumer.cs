using Common.Infrastructures.NpOn.RabbitMqExtMs.Generics;
using Common.Infrastructures.NpOn.RabbitMqExtMs.Receivers;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Events;
using MicroServices.Account.Service.NpOn.IAccountService;

namespace MicroServices.Account.Service.NpOn.AccountService.RabbitMqConsumers;

public class AccountSaveLoginRabbitMqConsumer(
    IAuthenticationService authenticationService,
    IRabbitMqConnection rabbitMqConnection,
    ILogger<AccountSaveLoginRabbitMqConsumer> logger,
    bool autoAck = true
) : RabbitMqConsumer<AccountSaveLoginEvent>(rabbitMqConnection, autoAck, logger)
{
    public override void AddHandler()
    {
        Handler = async (message)
            => await authenticationService.SaveLogin(message);
    }
}

public class AccountSaveLogoutRabbitMqConsumer(
    IAuthenticationService authenticationService,
    IRabbitMqConnection rabbitMqConnection,
    ILogger<AccountSaveLogoutRabbitMqConsumer> logger,
    bool autoAck = true
) : RabbitMqConsumer<AccountSaveLogoutEvent>(rabbitMqConnection, autoAck, logger)
{
    public override void AddHandler()
    {
        Handler = async (message)
            => await authenticationService.SaveLogout(message);
    }
}