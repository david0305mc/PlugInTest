using MessagePipe;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EMessage
{ 
    Test01,
    Test02,
}

public partial class MessageDispather
{

    public static IObservable<TMessage> Receive<TKey, TMessage>(TKey message)
    {
        return GetSubscriber<TKey, TMessage>().AsObservable(message);
    }

    public static IObservable<T> Receive<T>(EMessage message)
    {
        return Receive<EMessage, T>(message);
    }
}
