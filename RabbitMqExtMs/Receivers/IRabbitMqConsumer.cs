﻿using RabbitMqExtMs.Events;

namespace RabbitMqExtMs.Receivers;

public interface IRabbitMqConsumer
{
}

public interface IRabbitMqConsumer<T> : IRabbitMqConsumer where T : RabbitMqMessageContent
{
    public abstract void AddHandler();
}