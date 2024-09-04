using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    private void Awake()
    {
        //MessageDispather.Receive<int>(EMessage.Test01).Subscribe(_ =>
        //{
        //    RefreshWelcomeRewardNoti();
        //}).AddTo(gameObject);
    }
    void Start()
    {
    }

    private void TestMessagePipe()
    {
        var builder = new BuiltinContainerBuilder();
        builder.AddMessagePipe();
        builder.AddMessageBroker<int>();
        var provider = builder.BuildServiceProvider();
        GlobalMessagePipe.SetProvider(provider);

        //var p = provider.GetRequiredService<IPublisher<int>>();
        //var s1 = provider.GetRequiredService<ISubscriber<int>>();
        //var s2 = provider.GetRequiredService<ISubscriber<int>>();
        var p = GlobalMessagePipe.GetPublisher<int>();
        var s1 = GlobalMessagePipe.GetSubscriber<int>();
        var s2 = GlobalMessagePipe.GetSubscriber<int>();

        var d1 = s1.Subscribe(x => Debug.Log($"test 1 {x}"));
        var d2 = s2.Subscribe(x => Debug.Log($"test 2 {x}"));

        p.Publish(10);
        p.Publish(20);
    }
    public void SimpelePush()
    {
        var resolver = TestHelper.BuildBuiltin((builder) =>
        {
            builder.AddMessageBroker<int>();
        });
        GlobalMessagePipe.SetProvider(resolver);

        var pub = resolver.GetRequiredService<IPublisher<int>>();
        var sub1 = resolver.GetRequiredService<ISubscriber<int>>();
        var sub2 = resolver.GetRequiredService<ISubscriber<int>>();

        var list = new List<int>();
        var d1 = sub1.Subscribe(x => list.Add(x));
        var d2 = sub2.Subscribe(x => list.Add(x));

        pub.Publish(10);
        pub.Publish(20);

        list.Clear();
        d1.Dispose();

        pub.Publish(99);
    }
    public IEnumerator SimpleAsyncPush() => UniTask.ToCoroutine(async () =>
    {
        var resolver = TestHelper.BuildBuiltin((builder) =>
        {
            builder.AddMessageBroker<int>();
        });

        var pub = resolver.GetRequiredService<IAsyncPublisher<int>>();
        var sub1 = resolver.GetRequiredService<IAsyncSubscriber<int>>();
        var sub2 = resolver.GetRequiredService<IAsyncSubscriber<int>>();

        var list = new List<int>();
        var d1 = sub1.Subscribe(async (x, c) => { await UniTask.Delay(3000); list.Add(x); });
        var d2 = sub2.Subscribe(async (x, c) => { await UniTask.Yield(); list.Add(x); });

        await pub.PublishAsync(10);
        Debug.Log("PublishAsync");
        await pub.PublishAsync(20);
        Debug.Log("PublishAsync");

        list.Clear();
        d1.Dispose();

        await pub.PublishAsync(99);
    });

}
