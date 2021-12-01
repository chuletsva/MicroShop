﻿namespace EventBus.Abstractions;

public interface IEventBus
{
    void Publish<TEvent>(TEvent @event)
        where TEvent : IEvent;

    void Subscribe<TEvent, TEventHandler>()
        where TEvent : IEvent
        where TEventHandler : IEventHandler<TEvent>;

    void Unsubscribe<TEvent, TEventHandler>()
        where TEvent : IEvent
        where TEventHandler : IEventHandler<TEvent>;
}
