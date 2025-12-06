﻿using AccountServiceObject.EventObjects;
using IAccountService;
using RabbitMqExtMs.Generics;
using RabbitMqExtMs.Receivers;

namespace AccountService.RabbitMqConsumers;

public class AccountSaveLoginConsumer(
    IAuthenticationService authenticationService,
    IRabbitMqConnection rabbitMqConnection,
    ILogger<AccountSaveLoginConsumer> logger,
    bool autoAck = true
) : RabbitMqConsumer<AccountSaveLoginEvent>(rabbitMqConnection, autoAck, logger)
{
    public override void AddHandler()
    {
        Handler = async (message) 
                => await authenticationService.SaveLogin(message.ToObject());
    }
}
