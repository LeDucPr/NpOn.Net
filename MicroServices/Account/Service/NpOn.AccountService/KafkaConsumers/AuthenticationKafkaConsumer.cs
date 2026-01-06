using Common.Infrastructures.NpOn.KafkaExtCm.Receivers;
using Common.Infrastructures.NpOn.KafkaExtCm.Topics;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Events;
using MicroServices.Account.Service.NpOn.IAccountService;

namespace MicroServices.Account.Service.NpOn.AccountService.KafkaConsumers;

public class AccountSaveLoginKafkaConsumer(
    IAuthenticationService authenticationService,
    IKafkaTopic kafkaTopic,
    ILogger<AccountSaveLoginKafkaConsumer> logger,
    bool autoAck = true
) : KafkaConsumer<AccountSaveLoginEvent>(kafkaTopic, autoAck, logger)
{
    public override void AddHandler()
    {
        Handler = async (message)
            => await authenticationService.SaveLogin(message);
    }
}