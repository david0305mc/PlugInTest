using MessagePipe;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MessageDispather 
{
    private static MessagePipeProvider _provider;
    public static readonly MessageDispather Default = new MessageDispather();

    public MessageDispather()
    {
        _provider = new MessagePipeProvider();
        _provider.AddMessagePipe();

        GlobalMessagePipe.SetProvider(_provider);
    }

    public static IPublisher<TKey, TMessage> GetPublisher<TKey, TMessage>()
    {
        return _provider.GetService<IPublisher<TKey, TMessage>>();
    }

    public static void Publish<TKey, TMessage>(TKey message, TMessage data)
    {
        GetPublisher<TKey, TMessage>()?.Publish(message, data);
    }

    public static ISubscriber<TKey, TMessage> GetSubscriber<TKey, TMessage>()
    {
        var service = _provider.GetService<IPublisher<TKey, TMessage>>();
        if (service == null)
        {
            _provider.AddMessageBroker<TKey, TMessage>();
            service = _provider.GetService<IPublisher<TKey, TMessage>>();
        }

        return (ISubscriber<TKey, TMessage>)service;
    }

}
